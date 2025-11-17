using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.ASI.Basecode.Data.Migrations.InitFromRepo.Migrations
{
    /// <inheritdoc />
    public partial class InitFromRepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__Users__A9D1053413C382AE",
                schema: "ws",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "ws",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D1053413C382AE",
                schema: "ws",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__Users__A9D1053413C382AE",
                schema: "ws",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "ws",
                table: "Users",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D1053413C382AE",
                schema: "ws",
                table: "Users",
                column: "Email",
                unique: true);
        }
    }
}
