using Microsoft.EntityFrameworkCore.Migrations;

namespace BinmakBackEnd.Migrations
{
    public partial class freq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetNodeId",
                table: "Frequencies");

            migrationBuilder.AddColumn<int>(
                name: "KeyProcessAreaId",
                table: "Frequencies",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Frequencies_KeyProcessAreaId",
                table: "Frequencies",
                column: "KeyProcessAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Frequencies_KeyProcessAreas_KeyProcessAreaId",
                table: "Frequencies",
                column: "KeyProcessAreaId",
                principalTable: "KeyProcessAreas",
                principalColumn: "KeyProcessAreaId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Frequencies_KeyProcessAreas_KeyProcessAreaId",
                table: "Frequencies");

            migrationBuilder.DropIndex(
                name: "IX_Frequencies_KeyProcessAreaId",
                table: "Frequencies");

            migrationBuilder.DropColumn(
                name: "KeyProcessAreaId",
                table: "Frequencies");

            migrationBuilder.AddColumn<int>(
                name: "AssetNodeId",
                table: "Frequencies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
