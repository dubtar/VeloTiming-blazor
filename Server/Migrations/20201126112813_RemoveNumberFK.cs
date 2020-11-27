using Microsoft.EntityFrameworkCore.Migrations;

namespace VeloTiming.Server.Migrations
{
    public partial class RemoveNumberFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Numbers_Races_RaceId",
                table: "Numbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Riders_Numbers_NumberId",
                table: "Riders");

            migrationBuilder.DropIndex(
                name: "IX_Riders_NumberId",
                table: "Riders");

            migrationBuilder.DropIndex(
                name: "IX_Numbers_RaceId",
                table: "Numbers");

            migrationBuilder.DropColumn(
                name: "RaceId",
                table: "Numbers");

            migrationBuilder.RenameColumn(
                name: "NumberId",
                table: "Riders",
                newName: "Number");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Riders",
                newName: "NumberId");

            migrationBuilder.AddColumn<int>(
                name: "RaceId",
                table: "Numbers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Riders_NumberId",
                table: "Riders",
                column: "NumberId");

            migrationBuilder.CreateIndex(
                name: "IX_Numbers_RaceId",
                table: "Numbers",
                column: "RaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Numbers_Races_RaceId",
                table: "Numbers",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Riders_Numbers_NumberId",
                table: "Riders",
                column: "NumberId",
                principalTable: "Numbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
