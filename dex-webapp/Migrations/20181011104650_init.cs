using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace dex_webapp.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivateTokenEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    TransactionHash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    BlockNum = table.Column<long>(nullable: false),
                    GasPriceWei = table.Column<long>(nullable: false),
                    GasUsed = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivateTokenEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CancelEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TokenGet = table.Column<string>(nullable: true),
                    AmountGet = table.Column<int>(nullable: false),
                    TokenGive = table.Column<string>(nullable: true),
                    AmountGive = table.Column<int>(nullable: false),
                    Expires = table.Column<int>(nullable: false),
                    Nonce = table.Column<int>(nullable: false),
                    User = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    TransactionHash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    BlockNum = table.Column<long>(nullable: false),
                    GasPriceWei = table.Column<long>(nullable: false),
                    GasUsed = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancelEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeactivateTokenEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    TransactionHash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    BlockNum = table.Column<long>(nullable: false),
                    GasPriceWei = table.Column<long>(nullable: false),
                    GasUsed = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeactivateTokenEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepositEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true),
                    Amount = table.Column<int>(nullable: false),
                    Balance = table.Column<int>(nullable: false),
                    TransactionHash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    BlockNum = table.Column<long>(nullable: false),
                    GasPriceWei = table.Column<long>(nullable: false),
                    GasUsed = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TokenGet = table.Column<string>(nullable: true),
                    AmountGet = table.Column<int>(nullable: false),
                    TokenGive = table.Column<string>(nullable: true),
                    AmountGive = table.Column<int>(nullable: false),
                    Expires = table.Column<int>(nullable: false),
                    Nonce = table.Column<int>(nullable: false),
                    User = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    TransactionHash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    BlockNum = table.Column<long>(nullable: false),
                    GasPriceWei = table.Column<long>(nullable: false),
                    GasUsed = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    StatusDateUpdate = table.Column<DateTime>(nullable: false),
                    StatusBlockUpdate = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TokenGet = table.Column<string>(nullable: true),
                    AmountGet = table.Column<int>(nullable: false),
                    TokenGive = table.Column<string>(nullable: true),
                    AmountGive = table.Column<int>(nullable: false),
                    Get = table.Column<string>(nullable: true),
                    Give = table.Column<string>(nullable: true),
                    TransactionHash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    BlockNum = table.Column<long>(nullable: false),
                    GasPriceWei = table.Column<long>(nullable: false),
                    GasUsed = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true),
                    Amount = table.Column<int>(nullable: false),
                    Balance = table.Column<int>(nullable: false),
                    TransactionHash = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(nullable: true),
                    BlockNum = table.Column<long>(nullable: false),
                    GasPriceWei = table.Column<long>(nullable: false),
                    GasUsed = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CancelEvent_User",
                table: "CancelEvent",
                column: "User");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvent_User",
                table: "OrderEvent",
                column: "User");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivateTokenEvent");

            migrationBuilder.DropTable(
                name: "CancelEvent");

            migrationBuilder.DropTable(
                name: "DeactivateTokenEvent");

            migrationBuilder.DropTable(
                name: "DepositEvent");

            migrationBuilder.DropTable(
                name: "OrderEvent");

            migrationBuilder.DropTable(
                name: "Parameters");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "TradeEvent");

            migrationBuilder.DropTable(
                name: "WithdrawEvent");
        }
    }
}
