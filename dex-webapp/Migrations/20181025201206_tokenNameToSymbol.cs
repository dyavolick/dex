using Microsoft.EntityFrameworkCore.Migrations;

namespace dex_webapp.Migrations
{
    public partial class tokenNameToSymbol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Token",
                newName: "Symbol");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ActivateTokenEvent",
                newName: "Symbol");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "Token",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "ActivateTokenEvent",
                newName: "Name");
        }
    }
}
