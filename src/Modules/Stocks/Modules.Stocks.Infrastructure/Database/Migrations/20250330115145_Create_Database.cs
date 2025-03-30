using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Stocks.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Create_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stocks");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "stocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_search_results",
                schema: "stocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: false),
                    market_open = table.Column<string>(type: "text", nullable: false),
                    market_close = table.Column<string>(type: "text", nullable: false),
                    timezone = table.Column<string>(type: "text", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_search_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stocks",
                schema: "stocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stocks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_search_results_created_on_utc",
                schema: "stocks",
                table: "stock_search_results",
                column: "created_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_stock_search_results_ticker",
                schema: "stocks",
                table: "stock_search_results",
                column: "ticker");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_created_on_utc",
                schema: "stocks",
                table: "stocks",
                column: "created_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_ticker",
                schema: "stocks",
                table: "stocks",
                column: "ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "stocks");

            migrationBuilder.DropTable(
                name: "stock_search_results",
                schema: "stocks");

            migrationBuilder.DropTable(
                name: "stocks",
                schema: "stocks");
        }
    }
}
