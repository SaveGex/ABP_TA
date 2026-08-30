FROM mcr.microsoft.com/mssql/server:2022-latest

COPY src/Database/scripts/ /scripts/

EXPOSE 1433

ENTRYPOINT [ "/scripts/entrypoint.sh" ]
