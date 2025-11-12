using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    public partial class RemoveUserStringId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely drop the old string UserId column from Users if it exists
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Users','UserId') IS NOT NULL
BEGIN
    ALTER TABLE [ws].[Users] DROP COLUMN [UserId];
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-create the UserId column as nullable NVARCHAR(450) if it does not exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('ws.Users','UserId') IS NULL
BEGIN
    ALTER TABLE [ws].[Users] ADD [UserId] NVARCHAR(450) NULL;
END
");
        }
    }
}
