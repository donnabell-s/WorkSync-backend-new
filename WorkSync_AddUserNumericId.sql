IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF SCHEMA_ID(N'ws') IS NULL EXEC(N'CREATE SCHEMA [ws];');


IF COL_LENGTH('ws.Users','Id') IS NULL
BEGIN
    ALTER TABLE [ws].[Users] ADD [Id] INT NULL;
END



IF COL_LENGTH('ws.Bookings','UserRefId') IS NULL
BEGIN
    ALTER TABLE [ws].[Bookings] ADD [UserRefId] INT NULL;
END

IF COL_LENGTH('ws.BookingLogs','UserRefId') IS NULL
BEGIN
    ALTER TABLE [ws].[BookingLogs] ADD [UserRefId] INT NULL;
END

IF COL_LENGTH('ws.RoomLogs','UserRefId') IS NULL
BEGIN
    ALTER TABLE [ws].[RoomLogs] ADD [UserRefId] INT NULL;
END

IF COL_LENGTH('ws.Sessions','UserRefId') IS NULL
BEGIN
    ALTER TABLE [ws].[Sessions] ADD [UserRefId] INT NULL;
END

IF COL_LENGTH('ws.UserPreferences','UserRefId') IS NULL
BEGIN
    ALTER TABLE [ws].[UserPreferences] ADD [UserRefId] INT NULL;
END



IF EXISTS(SELECT 1 FROM [ws].[Users] WHERE Id IS NULL)
BEGIN
    ;WITH cte AS (
        SELECT UserId, ROW_NUMBER() OVER (ORDER BY Email, UserId) AS rn
        FROM [ws].[Users]
    )
    UPDATE u
    SET u.Id = c.rn
    FROM [ws].[Users] u
    JOIN cte c ON u.UserId = c.UserId;
END



UPDATE b
SET b.UserRefId = u.Id
FROM [ws].[Bookings] b
JOIN [ws].[Users] u ON b.UserId = u.UserId;

UPDATE bl
SET bl.UserRefId = u.Id
FROM [ws].[BookingLogs] bl
JOIN [ws].[Users] u ON bl.UserId = u.UserId;

UPDATE rl
SET rl.UserRefId = u.Id
FROM [ws].[RoomLogs] rl
JOIN [ws].[Users] u ON rl.UserId = u.UserId;

UPDATE s
SET s.UserRefId = u.Id
FROM [ws].[Sessions] s
JOIN [ws].[Users] u ON s.UserId = u.UserId;

UPDATE up
SET up.UserRefId = u.Id
FROM [ws].[UserPreferences] up
JOIN [ws].[Users] u ON up.UserId = u.UserId;



IF NOT EXISTS(SELECT 1 FROM [ws].[Users] WHERE Id IS NULL)
BEGIN
    ALTER TABLE [ws].[Users] ALTER COLUMN [Id] INT NOT NULL;
END



-- Bookings
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Bookings_Users_Id')
BEGIN
    IF NOT EXISTS(SELECT 1 FROM [ws].[Bookings] WHERE UserRefId IS NULL)
    BEGIN
        ALTER TABLE [ws].[Bookings] ALTER COLUMN [UserRefId] INT NOT NULL;
    END
    ALTER TABLE [ws].[Bookings] ADD CONSTRAINT FK_Bookings_Users_Id FOREIGN KEY (UserRefId) REFERENCES [ws].[Users](Id);
END

-- BookingLogs
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingLogs_Users_Id')
BEGIN
    IF NOT EXISTS(SELECT 1 FROM [ws].[BookingLogs] WHERE UserRefId IS NULL)
    BEGIN
        ALTER TABLE [ws].[BookingLogs] ALTER COLUMN [UserRefId] INT NOT NULL;
    END
    ALTER TABLE [ws].[BookingLogs] ADD CONSTRAINT FK_BookingLogs_Users_Id FOREIGN KEY (UserRefId) REFERENCES [ws].[Users](Id);
END

-- RoomLogs
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RoomLogs_Users_Id')
BEGIN
    IF NOT EXISTS(SELECT 1 FROM [ws].[RoomLogs] WHERE UserRefId IS NULL)
    BEGIN
        ALTER TABLE [ws].[RoomLogs] ALTER COLUMN [UserRefId] INT NOT NULL;
    END
    ALTER TABLE [ws].[RoomLogs] ADD CONSTRAINT FK_RoomLogs_Users_Id FOREIGN KEY (UserRefId) REFERENCES [ws].[Users](Id);
END

-- Sessions
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sessions_Users_Id')
BEGIN
    IF NOT EXISTS(SELECT 1 FROM [ws].[Sessions] WHERE UserRefId IS NULL)
    BEGIN
        ALTER TABLE [ws].[Sessions] ALTER COLUMN [UserRefId] INT NOT NULL;
    END
    ALTER TABLE [ws].[Sessions] ADD CONSTRAINT FK_Sessions_Users_Id FOREIGN KEY (UserRefId) REFERENCES [ws].[Users](Id);
END

-- UserPreferences
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserPreferences_Users_Id')
BEGIN
    IF NOT EXISTS(SELECT 1 FROM [ws].[UserPreferences] WHERE UserRefId IS NULL)
    BEGIN
        ALTER TABLE [ws].[UserPreferences] ALTER COLUMN [UserRefId] INT NOT NULL;
    END
    ALTER TABLE [ws].[UserPreferences] ADD CONSTRAINT FK_UserPreferences_Users_Id FOREIGN KEY (UserRefId) REFERENCES [ws].[Users](Id);
END



IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE o.name = 'Users' AND i.name = 'UQ__Users__A9D1053413C382AE')
BEGIN
    IF NOT EXISTS(SELECT 1 FROM sys.indexes i JOIN sys.tables t ON i.object_id = t.object_id WHERE i.name = 'UQ__Users__A9D1053413C382AE')
    BEGIN
        CREATE UNIQUE INDEX [UQ__Users__A9D1053413C382AE] ON [ws].[Users]([Email]);
    END
END


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251031125806_AddUserNumericId', N'9.0.6');

COMMIT;
GO

