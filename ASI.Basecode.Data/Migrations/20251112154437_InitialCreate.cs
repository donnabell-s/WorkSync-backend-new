using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ws");

            migrationBuilder.CreateTable(
                name: "Rooms",
                schema: "ws",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Seats = table.Column<int>(type: "int", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SizeLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OperatingHours = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Rooms__32863939FDF5413B", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "ws",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Fname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Lname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Role = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomAmenities",
                schema: "ws",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Amenity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAmenities", x => new { x.RoomId, x.Amenity });
                    table.ForeignKey(
                        name: "FK_RoomAmenities_Rooms",
                        column: x => x.RoomId,
                        principalSchema: "ws",
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "ws",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    UserRefId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StartDatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Recurrence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpectedAttendees = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Bookings__73951AED31FDD369", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Rooms",
                        column: x => x.RoomId,
                        principalSchema: "ws",
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                    table.ForeignKey(
                        name: "FK_Bookings_Users_Id",
                        column: x => x.UserRefId,
                        principalSchema: "ws",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoomLogs",
                schema: "ws",
                columns: table => new
                {
                    RoomLogId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    UserRefId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RoomLogs__71A264C4826A7346", x => x.RoomLogId);
                    table.ForeignKey(
                        name: "FK_RoomLogs_Rooms",
                        column: x => x.RoomId,
                        principalSchema: "ws",
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                    table.ForeignKey(
                        name: "FK_RoomLogs_Users_Id",
                        column: x => x.UserRefId,
                        principalSchema: "ws",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "ws",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Auth = table.Column<bool>(type: "bit", nullable: true),
                    UserRefId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sessions__C9F492906CEBC5BB", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_Id",
                        column: x => x.UserRefId,
                        principalSchema: "ws",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                schema: "ws",
                columns: table => new
                {
                    PrefId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRefId = table.Column<int>(type: "int", nullable: true),
                    BookingEmailConfirm = table.Column<bool>(type: "bit", nullable: true),
                    CancellationNotif = table.Column<bool>(type: "bit", nullable: true),
                    BookingReminder = table.Column<bool>(type: "bit", nullable: true),
                    ReminderTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    BookingDefaultMinutes = table.Column<int>(type: "int", nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserPref__1F832A20BF4E3852", x => x.PrefId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_Id",
                        column: x => x.UserRefId,
                        principalSchema: "ws",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookingLogs",
                schema: "ws",
                columns: table => new
                {
                    BookingLogId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    UserRefId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BookingL__D6D56B32605743C8", x => x.BookingLogId);
                    table.ForeignKey(
                        name: "FK_BookingLogs_Bookings",
                        column: x => x.BookingId,
                        principalSchema: "ws",
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                    table.ForeignKey(
                        name: "FK_BookingLogs_Users_Id",
                        column: x => x.UserRefId,
                        principalSchema: "ws",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingLogs_BookingId",
                schema: "ws",
                table: "BookingLogs",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingLogs_UserRefId",
                schema: "ws",
                table: "BookingLogs",
                column: "UserRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId",
                schema: "ws",
                table: "Bookings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserRefId",
                schema: "ws",
                table: "Bookings",
                column: "UserRefId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomLogs_RoomId",
                schema: "ws",
                table: "RoomLogs",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomLogs_UserRefId",
                schema: "ws",
                table: "RoomLogs",
                column: "UserRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserRefId",
                schema: "ws",
                table: "Sessions",
                column: "UserRefId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserRefId",
                schema: "ws",
                table: "UserPreferences",
                column: "UserRefId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D1053413C382AE",
                schema: "ws",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingLogs",
                schema: "ws");

            migrationBuilder.DropTable(
                name: "RoomAmenities",
                schema: "ws");

            migrationBuilder.DropTable(
                name: "RoomLogs",
                schema: "ws");

            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "ws");

            migrationBuilder.DropTable(
                name: "UserPreferences",
                schema: "ws");

            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "ws");

            migrationBuilder.DropTable(
                name: "Rooms",
                schema: "ws");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "ws");
        }
    }
}
