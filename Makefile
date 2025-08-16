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

.PHONY: test build clean db-* update-framework

test: testsresults.html
testsresults.html: vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll
	echo test results.html. $(netframework), $(servicename), $(connectionstr)
	dotnet test vassago.tests/vassago.tests.csproj --logger:"html;LogFileName=testsresults.html" --results-directory ./
vassago.tests/bin/$(configuration)/$(netframework)/vassago.tests.dll:vassago/bin/$(configuration)/$(netframework)/vassago.dll
	@echo tests.dll needed to build base vassago

build:vassago/bin/$(configuration)/$(netframework)/vassago.dll
vassago/bin/$(configuration)/$(netframework)/vassago.dll:
	dotnet build vassago/vassago.csproj
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
#must
#also
#be specified in the project file.
#
#microsoft. why. microsoft.
#do you understand the problem?

db-initial:
	sudo -u postgres psql -c "create database $(servicename)_dev;"
	sudo -u postgres psql -c "create user $(servicename) with encrypted password '$(pw_database)';"
	sudo -u postgres psql -c "grant all privileges on database ${servicename}_dev to $servicename;"
	sudo -u postgres psql -d "${servicename}_dev" -c "GRANT ALL ON SCHEMA public TO $servicename"

	cp appsettings.sample.json appsettings.json
	$(MAKE) db-update
db-update:
	dotnet ef database update --connection "$connnectionstr"
db-fullreset:
	sudo -u postgres psql -c "drop database ${servicename}_dev;"
	sudo -u postgres psql -c "drop user $servicename"
	$(MAKE) db-initial
