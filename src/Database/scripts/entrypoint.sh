#!bin/sh

set -e

/opt/mssql/bin/sqlservr --accept-eula &

echo "Waiting for SQL Server to start..."
until /opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U SA \
    -P "$MSSQL_SA_PASSWORD" \
    -Q "SELECT 1" \
    -C
do
    echo "Waiting..."
    sleep 2
done
echo "SQL Server is up and running!"


/scripts/init-db.sh

wait
