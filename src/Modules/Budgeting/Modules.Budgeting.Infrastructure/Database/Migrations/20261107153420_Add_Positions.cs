using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Budgeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Positions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "positions",
                schema: "budgeting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_positions", x => x.id);
                    table.CheckConstraint("CK_positions_quantity_NotNegative", "quantity >= 0");
                    table.CheckConstraint("CK_positions_total_cost_NotNegative", "total_cost >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_positions_user_id_ticker",
                schema: "budgeting",
                table: "positions",
                columns: new[] { "user_id", "ticker" },
                unique: true);

            // Tickers are upper-case everywhere else (stocks, SignalR groups, positions).
            migrationBuilder.Sql("UPDATE budgeting.transactions SET ticker = upper(trim(ticker)) WHERE ticker <> upper(trim(ticker));");

            // Backfill positions by replaying each user's trades per ticker in order, with the same
            // average-cost rules as Position.Buy/Sell (type 1 = buy/expense, 2 = sell/income;
            // money_amount is the per-share price). Oversold history is clamped at zero.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    r RECORD;
                    cur_user uuid := NULL;
                    cur_ticker text := NULL;
                    qty integer := 0;
                    cost numeric := 0;
                BEGIN
                    FOR r IN
                        SELECT user_id, ticker, type, quantity, money_amount
                        FROM budgeting.transactions
                        ORDER BY user_id, ticker, created_on_utc, id
                    LOOP
                        IF cur_user IS DISTINCT FROM r.user_id OR cur_ticker IS DISTINCT FROM r.ticker THEN
                            IF cur_user IS NOT NULL THEN
                                INSERT INTO budgeting.positions (id, user_id, ticker, quantity, total_cost, created_on_utc)
                                VALUES (gen_random_uuid(), cur_user, cur_ticker, qty, cost, now());
                            END IF;

                            cur_user := r.user_id;
                            cur_ticker := r.ticker;
                            qty := 0;
                            cost := 0;
                        END IF;

                        IF r.type = 1 THEN
                            qty := qty + r.quantity;
                            cost := round(cost + r.money_amount * r.quantity, 4);
                        ELSIF r.type = 2 THEN
                            IF r.quantity >= qty THEN
                                qty := 0;
                                cost := 0;
                            ELSE
                                cost := round(cost * (qty - r.quantity) / qty, 4);
                                qty := qty - r.quantity;
                            END IF;
                        END IF;
                    END LOOP;

                    IF cur_user IS NOT NULL THEN
                        INSERT INTO budgeting.positions (id, user_id, ticker, quantity, total_cost, created_on_utc)
                        VALUES (gen_random_uuid(), cur_user, cur_ticker, qty, cost, now());
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "positions",
                schema: "budgeting");
        }
    }
}
