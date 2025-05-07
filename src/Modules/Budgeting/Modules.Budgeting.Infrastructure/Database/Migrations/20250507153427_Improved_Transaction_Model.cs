using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Budgeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Improved_Transaction_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "limit_price",
                schema: "budgeting",
                table: "transactions",
                newName: "money_amount");

            migrationBuilder.AddColumn<string>(
                name: "money_currency",
                schema: "budgeting",
                table: "transactions",
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
                table: "transactions");

            migrationBuilder.RenameColumn(
                name: "money_amount",
                schema: "budgeting",
                table: "transactions",
                newName: "limit_price");
        }
    }
}
