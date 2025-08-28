##
# makefile
#
# @file
# @version 0.1

serviceusername=vassago
databasename=vassago_dev
pw_database=wnmhOttjA0wCiR9hVoG7jjrf90SxWvAV
connectionstr=Host=localhost;Database=${databasename};Username=${serviceusername};Password=${pw_database};IncludeErrorDetail=true;
netframework=net8.0
configuration=Debug

.PHONY: test TestResults/testsresults.html build clean db-* update-framework

test: TestResults/testsresults.html
TestResults/testsresults.html: vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll vassago/bin/$(configuration)/$(netframework)/vassago.dll vassago.tests/testdb-connectionstring.txt
	echo test results.html. $(netframework), $(serviceusername), $(connectionstr)
	rm -rf ./TestResults/
	dotnet test --blame-hang-timeout 10000 vassago.tests/vassago.tests.csproj --logger:"html;LogFileName=testsresults.html" --results-directory ./TestResults

vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll:vassago/bin/$(configuration)/$(netframework)/vassago.dll vassago.tests/*.cs
	@echo tests.dll needed to build base vassago
vassago.tests/testdb-connectionstring.txt:
	$(MAKE) db-setuptest
build:vassago/bin/$(configuration)/$(netframework)/vassago.dll
vassago/bin/$(configuration)/$(netframework)/vassago.dll: vassago/*.cs vassago/*.json
	dotnet build vassago/vassago.csproj
	cp -r vassago/bin/$(configuration)/$(netframework)/ dist
	@echo base vassago needed to build

clean:
	dotnet clean vassago
	dotnet clean vassago.tests
	sudo -u postgres psql <<< "SELECT 'DROP DATABASE ${databasename}_test' WHERE EXISTS (SELECT FROM pg_database WHERE datname = '${databasename}_test')\\gexec"
	rm -rf vassago/bin vassago/obj vassago.tests/bin vassago.tests/obj dist

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

# TODO: instead of sudoing all over the place, we should *just* be running as psql. or ourselves.
db-initial:
	sudo -u postgres psql -c "create database ${serviceusername}_dev;"
	sudo -u postgres psql -c "create user ${serviceusername} with encrypted password '$(pw_database)';"
	sudo -u postgres psql -c "grant all privileges on database ${databasename} to ${serviceusername};"
	sudo -u postgres psql -d "${databasename}" -c "GRANT ALL ON SCHEMA public TO ${serviceusername}"

	cp vassago/appsettings.sample.json vassago/appsettings.json
	$(MAKE) db-update
db-update:
	cd vassago; dotnet ef database update --connection "$(connectionstr)"
db-fullreset:
	sudo -u postgres psql -c "drop database ${databasename};"
	sudo -u postgres psql -c "drop user ${serviceusername}"
	$(MAKE) db-initial
db-addmigration:
	cd vassago; dotnet ef migrations add "$(migrationname)"
	cd vassago; dotnet ef database update --connection "${connectionstr}"
db-dump:
	sudo -u postgres pg_dump ${databasename} >dumpp
db-wipe:
	touch tables.csv
	chmod 777 tables.csv
	sudo -u postgres psql -d ${databasename} -c "select table_name from information_schema.tables where table_schema='public' AND table_name <> '__EFMigrationsHistory';" --csv -o tables.csv
	sed -i 1d tables.csv
	while read p; do sudo -u postgres psql -d ${databasename} -c "TRUNCATE \"$$p\" RESTART IDENTITY CASCADE;"; done<tables.csv
	rm tables.csv
db-setuptest: db-dump
# postgres may be love, postgres may be life, but it doesn't have "create database if not exists". or "drop if exists". or "wipe only data".
	sudo -u postgres psql <<< "SELECT 'DROP DATABASE ${databasename}_test' WHERE EXISTS (SELECT FROM pg_database WHERE datname = '${databasename}_test')\\gexec"
	sudo -u postgres psql -c "create database ${databasename}_test;"
	sudo -u postgres psql -c "grant all privileges on database ${databasename}_test to ${serviceusername};"
	sudo -u postgres psql -d "${databasename}_test" -c "GRANT ALL ON SCHEMA public TO ${serviceusername}"

	sudo -u postgres psql -d "${databasename}_test" -1 -f dumpp
	rm dumpp
	echo "Host=localhost;Database=${databasename}_test;Username=${serviceusername};Password=${pw_database};IncludeErrorDetail=true;" > vassago.tests/testdb-connectionstring.txt
