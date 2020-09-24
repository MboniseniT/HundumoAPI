using Microsoft.EntityFrameworkCore.Migrations;

namespace BinmakBackEnd.Migrations
{
    public partial class mathops : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MathematicalOperators",
                columns: table => new
                {
                    MathematicalOperatorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MathematicalOperatorSign = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MathematicalOperators", x => x.MathematicalOperatorId);
                });

            migrationBuilder.CreateTable(
                name: "FormulaCreations",
                columns: table => new
                {
                    FormulaCreationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyProcessAreaId = table.Column<int>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    MathematicalOperatorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaCreations", x => x.FormulaCreationId);
                    table.ForeignKey(
                        name: "FK_FormulaCreations_MathematicalOperators_MathematicalOperatorId",
                        column: x => x.MathematicalOperatorId,
                        principalTable: "MathematicalOperators",
                        principalColumn: "MathematicalOperatorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormulaCreations_MathematicalOperatorId",
                table: "FormulaCreations",
                column: "MathematicalOperatorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulaCreations");

            migrationBuilder.DropTable(
                name: "MathematicalOperators");
        }
    }
}
