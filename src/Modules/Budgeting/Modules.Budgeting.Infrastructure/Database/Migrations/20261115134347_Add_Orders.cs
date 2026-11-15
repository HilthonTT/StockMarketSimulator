using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Budgeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                schema: "budgeting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    side = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    trigger_price = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    filled_price = table.Column<decimal>(type: "numeric", nullable: true),
                    filled_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.CheckConstraint("CK_orders_quantity_Positive", "quantity > 0");
                    table.CheckConstraint("CK_orders_trigger_price_Positive", "trigger_price > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_status",
                schema: "budgeting",
                table: "orders",
                column: "status",
                filter: "status = 1");

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id_created_on_utc",
                schema: "budgeting",
                table: "orders",
                columns: new[] { "user_id", "created_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders",
                schema: "budgeting");
        }
    }
}
