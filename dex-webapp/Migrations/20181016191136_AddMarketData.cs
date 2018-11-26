using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace dex_webapp.Migrations
{
    public partial class AddMarketData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OHLCData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CurrencyId = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Range = table.Column<int>(nullable: false),
                    Min = table.Column<decimal>(nullable: false),
                    Max = table.Column<decimal>(nullable: false),
                    Open = table.Column<decimal>(nullable: false),
                    Close = table.Column<decimal>(nullable: false),
                    Volume = table.Column<decimal>(nullable: false),
                    VolumeBase = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OHLCData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OHLCData_Date",
                table: "OHLCData",
                column: "Date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OHLCData");
        }
    }
}
