using Microsoft.EntityFrameworkCore.Migrations;

namespace dex_webapp.Migrations
{
    public partial class RenameNameToSymbol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "DeactivateTokenEvent",
                newName: "Symbol");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "DeactivateTokenEvent",
                newName: "Name");
        }
    }
}
