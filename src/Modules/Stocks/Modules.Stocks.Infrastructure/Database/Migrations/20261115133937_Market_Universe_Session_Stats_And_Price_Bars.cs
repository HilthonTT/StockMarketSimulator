using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Stocks.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Market_Universe_Session_Stats_And_Price_Bars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "asset_class",
                schema: "stocks",
                table: "stocks",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "stock");

            migrationBuilder.AddColumn<decimal>(
                name: "day_high",
                schema: "stocks",
                table: "stocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "day_low",
                schema: "stocks",
                table: "stocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "stocks",
                table: "stocks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "open_price",
                schema: "stocks",
                table: "stocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "previous_close",
                schema: "stocks",
                table: "stocks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "sector",
                schema: "stocks",
                table: "stocks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "session_date",
                schema: "stocks",
                table: "stocks",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            // Existing rows start a session at their current price.
            migrationBuilder.Sql(
                """
                UPDATE stocks.stocks
                SET open_price = price,
                    previous_close = price,
                    day_high = price,
                    day_low = price,
                    session_date = (now() AT TIME ZONE 'utc')::date;
                """);

            migrationBuilder.CreateTable(
                name: "price_bars",
                schema: "stocks",
                columns: table => new
                {
                    ticker = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    bucket_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    open = table.Column<decimal>(type: "numeric", nullable: false),
                    high = table.Column<decimal>(type: "numeric", nullable: false),
                    low = table.Column<decimal>(type: "numeric", nullable: false),
                    close = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_bars", x => new { x.ticker, x.bucket_start });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_bars",
                schema: "stocks");

            migrationBuilder.DropColumn(
                name: "asset_class",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "day_high",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "day_low",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "open_price",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "previous_close",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "sector",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "session_date",
                schema: "stocks",
                table: "stocks");
        }
    }
}
