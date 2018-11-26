using Microsoft.EntityFrameworkCore.Migrations;

namespace dex_webapp.Migrations
{
    public partial class AddAmountAvailableField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmountAvailable",
                table: "OrderFilled",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountAvailable",
                table: "OrderFilled");
        }
    }
}
