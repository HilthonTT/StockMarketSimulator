using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Budgeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Added_TS_Vector_Search : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ticker",
                schema: "budgeting",
                table: "transactions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_ticker",
                schema: "budgeting",
                table: "transactions",
                column: "ticker")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_action_description",
                schema: "budgeting",
                table: "audit_logs",
                columns: new[] { "action", "description" })
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_ticker",
                schema: "budgeting",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_action_description",
                schema: "budgeting",
                table: "audit_logs");

            migrationBuilder.AlterColumn<string>(
                name: "ticker",
                schema: "budgeting",
                table: "transactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }
    }
}
