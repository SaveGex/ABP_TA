#!/bin/bash

/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "IF EXISTS (SELECT name FROM sys.databases WHERE name = '$MSSQL_DATABASE') AND EXISTS (SELECT name FROM sys.server_principals WHERE name = '$MSSQL_USER') SELECT 1 ELSE EXIT(1)" > /dev/null 2>&1

if [ $? -eq 0 ]; then
    exit 0
else
    exit 1
fi