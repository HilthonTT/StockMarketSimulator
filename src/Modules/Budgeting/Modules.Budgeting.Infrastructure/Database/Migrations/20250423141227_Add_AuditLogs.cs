using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Budgeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_AuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "budgeting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    log_type = table.Column<int>(type: "integer", nullable: false),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_entity_type = table.Column<string>(type: "text", nullable: true),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_is_deleted",
                schema: "budgeting",
                table: "audit_logs",
                column: "is_deleted",
                filter: "is_deleted = true");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_id",
                schema: "budgeting",
                table: "audit_logs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "budgeting");
        }
    }
}
