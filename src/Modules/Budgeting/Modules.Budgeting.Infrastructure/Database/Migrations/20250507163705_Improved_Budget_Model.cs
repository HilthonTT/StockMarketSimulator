using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Budgeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Improved_Budget_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "buying_power",
                schema: "budgeting",
                table: "budgets",
                newName: "money_amount");

            migrationBuilder.AddColumn<string>(
                name: "money_currency",
                schema: "budgeting",
                table: "budgets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "money_currency",
                schema: "budgeting",
                table: "budgets");

            migrationBuilder.RenameColumn(
                name: "money_amount",
                schema: "budgeting",
                table: "budgets",
                newName: "buying_power");
        }
    }
}
