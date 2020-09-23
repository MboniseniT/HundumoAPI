using Microsoft.EntityFrameworkCore.Migrations;

namespace BinmakBackEnd.Migrations
{
    public partial class colors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChoosen",
                table: "KeyProcessAreas");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "KeyProcessAreas",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "KeyProcessAreas",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ColorPalletes",
                columns: table => new
                {
                    ColorPalleteId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColorPalleteName = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorPalletes", x => x.ColorPalleteId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColorPalletes");

            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "KeyProcessAreas");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "KeyProcessAreas");

            migrationBuilder.AddColumn<bool>(
                name: "IsChoosen",
                table: "KeyProcessAreas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
