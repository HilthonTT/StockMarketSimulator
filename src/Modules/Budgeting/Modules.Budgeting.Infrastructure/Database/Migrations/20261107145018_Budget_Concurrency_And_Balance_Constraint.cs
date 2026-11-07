using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Budgeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Budget_Concurrency_And_Balance_Constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_buying_power_NotNegative",
                schema: "budgeting",
                table: "budgets");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "budgeting",
                table: "budgets",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddCheckConstraint(
                name: "CK_budgets_money_amount_NotNegative",
                schema: "budgeting",
                table: "budgets",
                sql: "money_amount >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_budgets_money_amount_NotNegative",
                schema: "budgeting",
                table: "budgets");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "budgeting",
                table: "budgets");

            migrationBuilder.AddCheckConstraint(
                name: "CK_buying_power_NotNegative",
                schema: "budgeting",
                table: "budgets",
                sql: "buying_power > 0");
        }
    }
}
