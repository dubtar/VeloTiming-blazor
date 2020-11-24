using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VeloTiming.Server.Migrations
{
    public partial class OtherTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Numbers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NumberRfids = table.Column<string>(type: "TEXT", nullable: true),
                    RaceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Numbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Numbers_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RaceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    Sex = table.Column<int>(type: "INTEGER", nullable: true),
                    MinYearOfBirth = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxYearOfBirth = table.Column<int>(type: "INTEGER", nullable: true),
                    RaceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceCategories_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Starts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    PlannedStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RealStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    End = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RaceId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DelayMarksAfterStartMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Starts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Starts_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Riders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Sex = table.Column<int>(type: "INTEGER", nullable: false),
                    YearOfBirth = table.Column<int>(type: "INTEGER", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    Team = table.Column<string>(type: "TEXT", nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    RaceId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Riders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Riders_Numbers_NumberId",
                        column: x => x.NumberId,
                        principalTable: "Numbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Riders_RaceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "RaceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Riders_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeSource = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Number = table.Column<string>(type: "TEXT", nullable: true),
                    NumberSource = table.Column<string>(type: "TEXT", nullable: true),
                    IsIgnored = table.Column<bool>(type: "INTEGER", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Lap = table.Column<int>(type: "INTEGER", nullable: false),
                    Place = table.Column<int>(type: "INTEGER", nullable: false),
                    StartId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_Starts_StartId",
                        column: x => x.StartId,
                        principalTable: "Starts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StartCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartCategory_RaceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "RaceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StartCategory_Starts_StartId",
                        column: x => x.StartId,
                        principalTable: "Starts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Numbers_RaceId",
                table: "Numbers",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceCategories_RaceId",
                table: "RaceCategories",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_StartId",
                table: "Results",
                column: "StartId");

            migrationBuilder.CreateIndex(
                name: "IX_Riders_CategoryId",
                table: "Riders",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Riders_NumberId",
                table: "Riders",
                column: "NumberId");

            migrationBuilder.CreateIndex(
                name: "IX_Riders_RaceId",
                table: "Riders",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_StartCategory_CategoryId",
                table: "StartCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StartCategory_StartId",
                table: "StartCategory",
                column: "StartId");

            migrationBuilder.CreateIndex(
                name: "IX_Starts_RaceId",
                table: "Starts",
                column: "RaceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Riders");

            migrationBuilder.DropTable(
                name: "StartCategory");

            migrationBuilder.DropTable(
                name: "Numbers");

            migrationBuilder.DropTable(
                name: "RaceCategories");

            migrationBuilder.DropTable(
                name: "Starts");
        }
    }
}
