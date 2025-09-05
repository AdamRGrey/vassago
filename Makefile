##
# makefile
#
# @file
# @version 0.1
#
export
serviceusername=vassago
databasename=vassago_dev
pw_database=wnmhOttjA0wCiR9hVoG7jjrf90SxWvAV
netframework=net8.0
configuration=Debug
CONNECTIONSTR=Host=localhost;Database=${databasename};Username=${serviceusername};Password=${pw_database};IncludeErrorDetail=true;

.PHONY: clean build test sniff TestResults/testsresults.html db-* update-framework CONNECTIONSTR.txt

build: should-dbupdate vassago/bin/$(configuration)/$(netframework)/vassago.dll
	cp -r vassago/bin/$(configuration)/$(netframework)/ dist
	@echo base vassago needed to build

sniff:
	@echo "dotnet will 'handle' exceptions in anonymous functions. I've had it with trying to track them down."
	rg -i "\) =>" -g '*.cs' -g '!vassago/Program.cs';  test $$? -eq 1
test: TestResults/testsresults.html
	@echo "hello from test (itself). $(CONNECTIONSTR), in case you were wondering."
TestResults/testsresults.html: vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll vassago/bin/$(configuration)/$(netframework)/vassago.dll vassago.tests/testdb-connectionstring.txt
	@echo test results.html. $(netframework), $(serviceusername), $(CONNECTIONSTR)
	rm -rf ./TestResults/
	dotnet test --configuration $(configuration) --blame-hang-timeout 10000 vassago.tests/vassago.tests.csproj --logger:"html;LogFileName=testsresults.html" --results-directory ./TestResults

vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll:vassago/bin/$(configuration)/$(netframework)/vassago.dll vassago.tests/*.cs vassago.tests/vassago.tests.csproj
	@echo tests.dll needed to build base vassago

vassago/bin/$(configuration)/$(netframework)/vassago.dll: vassago/*.cs vassago/vassago.csproj
	dotnet build --configuration $(configuration) vassago/vassago.csproj
clean:
	@echo "hi i am the clean target, I will not be building anything."
	dotnet clean vassago
	dotnet clean vassago.tests
	rm -rf vassago/bin vassago/obj vassago.tests/bin vassago.tests/obj dist
	psql -c "DROP DATABASE IF EXISTS ${databasename}_test"

update-framework:
	@echo updating framework to $(netframework)
	sed -i 's/<TargetFramework>.*<\/TargetFramework>/<TargetFramework>$(netframework)<\/TargetFramework>/' vassago/vassago.csproj
	sed -i 's/<TargetFramework>.*<\/TargetFramework>/<TargetFramework>$(netframework)<\/TargetFramework>/' vassago.tests/vassago.tests.csproj
# "but adam, doesn't dotnet let you specify the framework to build with?" yes, but... this is from `dotnet build --help`:
#-f, --framework <FRAMEWORK>          The target framework to build for. The target framework must also be specified in the project file.
#to reiterate:
#The target framework
# must
# also
#be specified in the project file.
#
#microsoft. why. microsoft. do you understand the problem, microsoft? i'm worried you don't think this is an absurd thing to have done.
#
should-dbupdate: vassago/Migrations/ChattingContextModelSnapshot.cs
	@echo "hi i'm should-dbupdate. connection string: ${CONNECTIONSTR}"
	cd vassago; dotnet ef database update --connection "${CONNECTIONSTR}"


db-initial:
	psql -c "create database ${serviceusername};"
	psql -c "create user ${serviceusername} with encrypted password '$(pw_database)';"
	psql -c "grant all privileges on database ${databasename} to ${serviceusername};"
	psql -d "${databasename}" -c "GRANT ALL ON SCHEMA public TO ${serviceusername}"

	cp vassago/appsettings.sample.json vassago/appsettings.json
db-update:
	@echo "hi i'm the db-update target. connection string: ${CONNECTIONSTR}"
	cd vassago; dotnet ef database update --connection "${CONNECTIONSTR}"
db-fullreset:
	psql -c "drop database ${databasename};"
	psql -c "drop user ${serviceusername}"
	@echo "now do make db-initial."
db-addmigration:
	cd vassago; dotnet ef migrations add "$(migrationname)"
	cd vassago; dotnet ef database update --connection "${CONNECTIONSTR}"
db-dump:
	pg_dump ${databasename} >dumpp
db-recover:
	psql -d "${databasename}" -1 -f dumpp
db-wipe:
	touch tables.csv
	chmod 777 tables.csv
	psql -d ${databasename} -c "select table_name from information_schema.tables where table_schema='public' AND table_name <> '__EFMigrationsHistory';" --csv -o tables.csv
	sed -i 1d tables.csv
	while read p; do psql -d ${databasename} -c "TRUNCATE \"$$p\" RESTART IDENTITY CASCADE;"; done<tables.csv
	rm tables.csv
db-setuptest: db-dump
	psql -c "DROP DATABASE IF EXISTS ${databasename}_test"
	psql -c "create database ${databasename}_test;"
	psql -c "grant all privileges on database ${databasename}_test to ${serviceusername};"
	psql -d "${databasename}_test" -c "GRANT ALL ON SCHEMA public TO ${serviceusername}"

	psql -d "${databasename}_test" -1 -f dumpp
	rm dumpp
	@echo "you should probably now make testdb-connectionstring.txt. I would, but, you know. FOR NOW, pw=${pw_database}"
#$(MAKE) vassago.tests/testdb-connectionstring.txt
vassago.tests/testdb-connectionstring.txt:
	echo "Host=localhost;Database=${databasename}_test;Username=${serviceusername};Password=${pw_database};IncludeErrorDetail=true;" > vassago.tests/testdb-connectionstring.txt
