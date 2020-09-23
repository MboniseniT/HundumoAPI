using Microsoft.EntityFrameworkCore.Migrations;

namespace BinmakBackEnd.Migrations
{
    public partial class production : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "Productions");

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "Productions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "KeyProcessAreas",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "Productions");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "KeyProcessAreas");

            migrationBuilder.AddColumn<int>(
                name: "TargetValue",
                table: "Productions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
