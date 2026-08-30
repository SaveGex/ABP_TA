#!bin/sh
set -e

until
/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U SA \
    -P "$MSSQL_SA_PASSWORD" \
    -i /scripts/init.sql \
    -C;
do
    echo "Waiting for SQL Server..."
    sleep 5
done
