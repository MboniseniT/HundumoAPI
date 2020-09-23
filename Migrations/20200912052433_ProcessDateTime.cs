using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BinmakBackEnd.Migrations
{
    public partial class ProcessDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChoosen",
                table: "Processes");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "Processes",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "Processes");

            migrationBuilder.AddColumn<bool>(
                name: "IsChoosen",
                table: "Processes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
