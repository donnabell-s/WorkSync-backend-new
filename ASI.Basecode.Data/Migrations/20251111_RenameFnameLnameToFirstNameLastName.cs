using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    public partial class RenameFnameLnameToFirstNameLastName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns FirstName, LastName
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "ws",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "ws",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Copy existing data from Fname/Lname into the new columns if present
            migrationBuilder.Sql(@"UPDATE [ws].[Users] SET [FirstName] = [Fname], [LastName] = [Lname] WHERE [FirstName] IS NULL AND [LastName] IS NULL");

            // Optional: keep old columns for backward compatibility. Do not drop them to avoid breaking existing code.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Copy data back
            migrationBuilder.Sql(@"UPDATE [ws].[Users] SET [Fname] = [FirstName], [Lname] = [LastName] WHERE [Fname] IS NULL AND [Lname] IS NULL");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "ws",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "ws",
                table: "Users");
        }
    }
}
