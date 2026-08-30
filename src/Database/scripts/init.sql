-- Ensure the database exists
IF NOT EXISTS (
    SELECT name
    FROM sys.databases
    WHERE name = '$(MSSQL_DATABASE)'
)
BEGIN
    PRINT 'Creating database $(MSSQL_DATABASE)...';
    CREATE DATABASE [$(MSSQL_DATABASE)];
END
ELSE
BEGIN
    PRINT 'Database $(MSSQL_DATABASE) already exists.';
END
GO

-- Check if the login exists at the server level
IF NOT EXISTS (
    SELECT 1
    FROM sys.server_principals
    WHERE name = '$(MSSQL_USER)'
)
BEGIN
    PRINT 'Creating server login for $(MSSQL_USER)...';
    CREATE LOGIN [$(MSSQL_USER)] WITH PASSWORD = '$(MSSQL_USER_PASSWORD)';
END
GO

WAITFOR DELAY '00:00:02';
-- Switch to the newly created database after ensuring LOGIN exists
USE [$(MSSQL_DATABASE)]
GO

IF NOT EXISTS(
    SELECT 1
    FROM sys.database_principals
    WHERE name = '$(MSSQL_USER)'
)
BEGIN
    PRINT 'Creating user $(MSSQL_USER)...';
    CREATE USER [$(MSSQL_USER)] FOR LOGIN [$(MSSQL_USER)];
END
ELSE
BEGIN
    PRINT 'User $(MSSQL_USER) already exists.';
END
GO

-- Add role
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
    WHERE r.name = 'db_owner'
      AND m.name = '$(MSSQL_USER)'
)
BEGIN
    PRINT 'Adding $(MSSQL_USER) to db_owner role';
    ALTER ROLE db_owner ADD MEMBER [$(MSSQL_USER)];
END
GO

-- As well as EF Core could figure out by himself that DB is already created
PRINT 'Granting VIEW ANY DATABASE permission to $(MSSQL_USER)...';
USE [master]
GO
GRANT VIEW ANY DATABASE TO [$(MSSQL_USER)];
GO

PRINT 'Done';
