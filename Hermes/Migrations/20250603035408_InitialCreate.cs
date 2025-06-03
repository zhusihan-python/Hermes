using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hermes.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Department = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Heartbeat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DevReset = table.Column<byte>(type: "INTEGER", nullable: false),
                    SealMotRst = table.Column<byte>(type: "INTEGER", nullable: false),
                    SortMotRst = table.Column<byte>(type: "INTEGER", nullable: false),
                    SealMotFlow = table.Column<byte>(type: "INTEGER", nullable: false),
                    SortMotFlow = table.Column<byte>(type: "INTEGER", nullable: false),
                    SysActStat = table.Column<byte>(type: "INTEGER", nullable: false),
                    ScanTgt = table.Column<ushort>(type: "INTEGER", nullable: false),
                    ActPackSeq = table.Column<ushort>(type: "INTEGER", nullable: false),
                    ActPackRem = table.Column<ushort>(type: "INTEGER", nullable: false),
                    MotBrd1Stat = table.Column<byte>(type: "INTEGER", nullable: false),
                    MotBrd2Stat = table.Column<byte>(type: "INTEGER", nullable: false),
                    EnvBrdStat = table.Column<byte>(type: "INTEGER", nullable: false),
                    GasPress = table.Column<float>(type: "REAL", nullable: false),
                    Suck1Press = table.Column<float>(type: "REAL", nullable: false),
                    Suck2Press = table.Column<float>(type: "REAL", nullable: false),
                    BakeStat = table.Column<byte>(type: "INTEGER", nullable: false),
                    BakeTgtTemp = table.Column<float>(type: "REAL", nullable: false),
                    BakeCurrTemp = table.Column<float>(type: "REAL", nullable: false),
                    BakeTgtTime = table.Column<uint>(type: "INTEGER", nullable: false),
                    BakeRemTime = table.Column<uint>(type: "INTEGER", nullable: false),
                    WasteBoxIn = table.Column<byte>(type: "INTEGER", nullable: false),
                    CoverBoxIn = table.Column<byte>(type: "INTEGER", nullable: false),
                    CoverRemCnt = table.Column<ushort>(type: "INTEGER", nullable: false),
                    SlideBoxIn = table.Column<byte[]>(type: "BLOB", maxLength: 10, nullable: false),
                    SlideInInfo = table.Column<byte[]>(type: "BLOB", maxLength: 188, nullable: false),
                    SlideActStat = table.Column<byte[]>(type: "BLOB", maxLength: 75, nullable: false),
                    SensTemp = table.Column<float>(type: "REAL", nullable: false),
                    SensHumi = table.Column<float>(type: "REAL", nullable: false),
                    SealLiq = table.Column<byte>(type: "INTEGER", nullable: false),
                    XyleneLiq = table.Column<byte>(type: "INTEGER", nullable: false),
                    CleanLiq = table.Column<byte>(type: "INTEGER", nullable: false),
                    Ts = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heartbeat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Record",
                columns: table => new
                {
                    RecordId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    RecordType = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Record", x => x.RecordId);
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
                name: "Slides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProgramId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PathologyId = table.Column<int>(type: "INTEGER", nullable: false),
                    SlideId = table.Column<int>(type: "INTEGER", maxLength: 16, nullable: false),
                    PatientName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DoctorId = table.Column<int>(type: "INTEGER", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SealState = table.Column<byte>(type: "INTEGER", nullable: false),
                    SortState = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slides_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Slides_DoctorId",
                table: "Slides",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Heartbeat");

            migrationBuilder.DropTable(
                name: "Record");

            migrationBuilder.DropTable(
                name: "Slides");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Doctors");
        }
    }
}
