using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hermes.AppData.Migrations
{
    /// <inheritdoc />
    public partial class _001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnitsUnderTest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    IsFail = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsUnderTest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Defects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorFlag = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ErrorCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UnitUnderTestId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Defects_UnitsUnderTest_UnitUnderTestId",
                        column: x => x.UnitUnderTestId,
                        principalTable: "UnitsUnderTest",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SfcResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UnitUnderTestId = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorType = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfcResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SfcResponses_UnitsUnderTest_UnitUnderTestId",
                        column: x => x.UnitUnderTestId,
                        principalTable: "UnitsUnderTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SfcResponseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRestored = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stops_SfcResponses_SfcResponseId",
                        column: x => x.SfcResponseId,
                        principalTable: "SfcResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Defects_UnitUnderTestId",
                table: "Defects",
                column: "UnitUnderTestId");

            migrationBuilder.CreateIndex(
                name: "IX_SfcResponses_UnitUnderTestId",
                table: "SfcResponses",
                column: "UnitUnderTestId");

            migrationBuilder.CreateIndex(
                name: "IX_Stops_SfcResponseId",
                table: "Stops",
                column: "SfcResponseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Defects");

            migrationBuilder.DropTable(
                name: "Stops");

            migrationBuilder.DropTable(
                name: "SfcResponses");

            migrationBuilder.DropTable(
                name: "UnitsUnderTest");
        }
    }
}
