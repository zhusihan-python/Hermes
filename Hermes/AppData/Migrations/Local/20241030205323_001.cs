using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hermes.AppData.Migrations.Local
{
    /// <inheritdoc />
    public partial class _001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SfcResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false),
                    FullPath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfcResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRestored = table.Column<bool>(type: "INTEGER", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    Actions = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Department = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitsUnderTest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    IsFail = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StopId = table.Column<int>(type: "INTEGER", nullable: true),
                    SfcResponseId = table.Column<int>(type: "INTEGER", nullable: true),
                    FullPath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsUnderTest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitsUnderTest_SfcResponses_SfcResponseId",
                        column: x => x.SfcResponseId,
                        principalTable: "SfcResponses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnitsUnderTest_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StopUser",
                columns: table => new
                {
                    StopsId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopUser", x => new { x.StopsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_StopUser_Stops_StopsId",
                        column: x => x.StopsId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StopUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Defects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StopId = table.Column<int>(type: "INTEGER", nullable: true),
                    UnitUnderTestId = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorFlag = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ErrorCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Defects_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Defects_UnitsUnderTest_UnitUnderTestId",
                        column: x => x.UnitUnderTestId,
                        principalTable: "UnitsUnderTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Defects_StopId",
                table: "Defects",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_Defects_UnitUnderTestId",
                table: "Defects",
                column: "UnitUnderTestId");

            migrationBuilder.CreateIndex(
                name: "IX_StopUser_UsersId",
                table: "StopUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitsUnderTest_SfcResponseId",
                table: "UnitsUnderTest",
                column: "SfcResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitsUnderTest_StopId",
                table: "UnitsUnderTest",
                column: "StopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Defects");

            migrationBuilder.DropTable(
                name: "StopUser");

            migrationBuilder.DropTable(
                name: "UnitsUnderTest");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SfcResponses");

            migrationBuilder.DropTable(
                name: "Stops");
        }
    }
}
