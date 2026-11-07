using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Stocks.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Unique_Stock_Ticker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stocks_ticker",
                schema: "stocks",
                table: "stocks");

            // Deduplicate before the unique index can be created: per (upper-cased) ticker keep the most
            // recently written row, which is the price the feed and the trades were using.
            migrationBuilder.Sql(
                """
                DELETE FROM stocks.stocks AS s
                USING (
                    SELECT id,
                           ROW_NUMBER() OVER (
                               PARTITION BY upper(trim(ticker))
                               ORDER BY COALESCE(modified_on_utc, created_on_utc) DESC, created_on_utc DESC, id DESC
                           ) AS rank
                    FROM stocks.stocks
                ) AS ranked
                WHERE s.id = ranked.id AND ranked.rank > 1;

                UPDATE stocks.stocks
                SET ticker = upper(trim(ticker))
                WHERE ticker <> upper(trim(ticker));
                """);

            migrationBuilder.CreateIndex(
                name: "ix_stocks_ticker",
                schema: "stocks",
                table: "stocks",
                column: "ticker",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stocks_ticker",
                schema: "stocks",
                table: "stocks");

            migrationBuilder.CreateIndex(
                name: "ix_stocks_ticker",
                schema: "stocks",
                table: "stocks",
                column: "ticker");
        }
    }
}
