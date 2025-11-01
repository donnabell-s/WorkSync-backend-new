using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNumericId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure schema exists
            migrationBuilder.EnsureSchema(name: "ws");

            // Add numeric Id column to Users if it doesn't exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Users','Id') IS NULL
BEGIN
    ALTER TABLE [ws].[Users] ADD [Id] INT NULL;
END
");

            // Add new integer FK columns on dependent tables if they don't exist
            migrationBuilder.Sql(@"
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
");

            // Populate Users.Id with sequential values if any NULLs exist
            migrationBuilder.Sql(@"
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
");

            // Populate new FK columns by joining on existing string UserId
            migrationBuilder.Sql(@"
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
");

            // Make Users.Id NOT NULL if all populated
            migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT 1 FROM [ws].[Users] WHERE Id IS NULL)
BEGIN
    ALTER TABLE [ws].[Users] ALTER COLUMN [Id] INT NOT NULL;
END
");

            // Make new FK columns NOT NULL when safe and add FK constraints conditionally
            migrationBuilder.Sql(@"
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
");

            // Optionally create an index on Users.Email if missing
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes i JOIN sys.objects o ON i.object_id = o.object_id WHERE o.name = 'Users' AND i.name = 'UQ__Users__A9D1053413C382AE')
BEGIN
    IF NOT EXISTS(SELECT 1 FROM sys.indexes i JOIN sys.tables t ON i.object_id = t.object_id WHERE i.name = 'UQ__Users__A9D1053413C382AE')
    BEGIN
        CREATE UNIQUE INDEX [UQ__Users__A9D1053413C382AE] ON [ws].[Users]([Email]);
    END
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove FK constraints if they exist
            migrationBuilder.Sql(@"
IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Bookings_Users_Id')
BEGIN
    ALTER TABLE [ws].[Bookings] DROP CONSTRAINT FK_Bookings_Users_Id;
END

IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BookingLogs_Users_Id')
BEGIN
    ALTER TABLE [ws].[BookingLogs] DROP CONSTRAINT FK_BookingLogs_Users_Id;
END

IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RoomLogs_Users_Id')
BEGIN
    ALTER TABLE [ws].[RoomLogs] DROP CONSTRAINT FK_RoomLogs_Users_Id;
END

IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sessions_Users_Id')
BEGIN
    ALTER TABLE [ws].[Sessions] DROP CONSTRAINT FK_Sessions_Users_Id;
END

IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserPreferences_Users_Id')
BEGIN
    ALTER TABLE [ws].[UserPreferences] DROP CONSTRAINT FK_UserPreferences_Users_Id;
END
");

            // Remove added FK columns if they exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Bookings','UserRefId') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[Bookings] DROP COLUMN [UserRefId];
END

IF COL_LENGTH('ws.BookingLogs','UserRefId') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[BookingLogs] DROP COLUMN [UserRefId];
END

IF COL_LENGTH('ws.RoomLogs','UserRefId') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[RoomLogs] DROP COLUMN [UserRefId];
END

IF COL_LENGTH('ws.Sessions','UserRefId') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[Sessions] DROP COLUMN [UserRefId];
END

IF COL_LENGTH('ws.UserPreferences','UserRefId') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[UserPreferences] DROP COLUMN [UserRefId];
END
");

            // Remove Users.Id column if it exists
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Users','Id') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[Users] DROP COLUMN [Id];
END
");

            // Optionally drop the unique index created on Email
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ__Users__A9D1053413C382AE')
BEGIN
    DROP INDEX [UQ__Users__A9D1053413C382AE] ON [ws].[Users];
END
");
        }
    }
}
