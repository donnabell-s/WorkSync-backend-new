using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    public partial class ForceAddUserRefId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure UserRefId exists on Bookings and other tables
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

            // Attempt to populate UserRefId from Users.Id if possible
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Users','Id') IS NOT NULL
BEGIN
    UPDATE b
    SET b.UserRefId = u.Id
    FROM [ws].[Bookings] b
    JOIN [ws].[Users] u ON b.UserId = u.UserId
    WHERE b.UserRefId IS NULL AND u.Id IS NOT NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
