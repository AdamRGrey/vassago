##
# makefile
#
# @file
# @version 0.1

servicename=vassago
pw_database=wnmhOttjA0wCiR9hVoG7jjrf90SxWvAV
connectionstr=Host=localhost;Database=${servicename}_dev;Username=${servicename};Password=${pw_database};IncludeErrorDetail=true;
netframework=net8.0
configuration=Debug

.PHONY: test TestResults/testsresults.html build clean db-* update-framework

test: TestResults/testsresults.html
TestResults/testsresults.html: vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll
	echo test results.html. $(netframework), $(servicename), $(connectionstr)
	rm -rf ./TestResults/
	dotnet test --blame-hang-timeout 10000 vassago.tests/vassago.tests.csproj --logger:"html;LogFileName=testsresults.html" --results-directory ./TestResults
vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll:vassago/bin/$(configuration)/$(netframework)/vassago.dll vassago.tests/*.cs
	@echo tests.dll needed to build base vassago

build:vassago/bin/$(configuration)/$(netframework)/vassago.dll
vassago/bin/$(configuration)/$(netframework)/vassago.dll: vassago/*.cs vassago/*.json
	dotnet build vassago/vassago.csproj
	cp vassago/bin/$(configuration)/$(netframework)/ dist
	@echo base vassago needed to build

clean:
	dotnet clean vassago
	dotnet clean vassago.tests
	rm -rf vassago/bin vassago/obj vassago.tests/bin vassago.tests/obj

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

db-initial:
	sudo -u postgres psql -c "create database $(servicename)_dev;"
	sudo -u postgres psql -c "create user $(servicename) with encrypted password '$(pw_database)';"
	sudo -u postgres psql -c "grant all privileges on database ${servicename}_dev to $servicename;"
	sudo -u postgres psql -d "${servicename}_dev" -c "GRANT ALL ON SCHEMA public TO $servicename"

	cp vassago/appsettings.sample.json vassago/appsettings.json
	$(MAKE) db-update
db-update:
	cd vassago; dotnet ef database update --connection "$(connectionstr)"
db-fullreset:
	sudo -u postgres psql -c "drop database ${servicename}_dev;"
	sudo -u postgres psql -c "drop user $servicename"
	$(MAKE) db-initial
db-addmigration:
	cd vassago; dotnet ef migrations add "$(migrationname)"
	cd vassago; dotnet ef database update --connection "$(connectionstr)"
db-dump:
	sudo -u postgres pg_dump ${servicename}_dev >dumpp
db-wipe:
	touch tables.csv
	chmod 777 tables.csv
	sudo -u postgres psql -d ${servicename}_dev -c "select table_name from information_schema.tables where table_schema='public' AND table_name <> '__EFMigrationsHistory';" --csv -o tables.csv
	sed -i 1d tables.csv
	while read p; do sudo -u postgres psql -d vassago_dev -c "TRUNCATE \"$$p\" RESTART IDENTITY CASCADE;"; done<tables.csv
	rm tables.csv
