using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    public partial class AddRoomImageUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "ws");

            // Add ImageUrl column if it does not already exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Rooms','ImageUrl') IS NULL
BEGIN
    ALTER TABLE [ws].[Rooms] ADD [ImageUrl] VARCHAR(500) NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Rooms','ImageUrl') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[Rooms] DROP COLUMN [ImageUrl];
END
");
        }
    }
}
