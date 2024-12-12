#!/bin/bash

servicename="vassago"
pw_developmentdatabase="wnmhOttjA0wCiR9hVoG7jjrf90SxWvAV"
connnectionstr="Host=localhost;Database=${servicename}_dev;Username=${servicename};Password=${pw_developmentdatabase};IncludeErrorDetail=true;"

case "$1" in
    "dbinit")
		sudo -u postgres psql -c "create database ${servicename}_dev;"
		sudo -u postgres psql -c "create user $servicename with encrypted password '$pw_developmentdatabase';"
		sudo -u postgres psql -c "grant all privileges on database ${servicename}_dev to $servicename;"
		sudo -u postgres psql -d "${servicename}_dev" -c "GRANT ALL ON SCHEMA public TO $servicename"

		#dotnet ef dbcontext scaffold "$connnectionstr" Npgsql.EntityFrameworkCore.PostgreSQL
		;;

    "dbupdate")
        dotnet ef migrations add "$2"
		dotnet ef database update --connection "$connnectionstr";;

    *)
        echo "Unknown command '$1', try 'dbinit'" ;;
esac


