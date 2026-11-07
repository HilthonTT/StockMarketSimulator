using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Fix_OutboxConsumer_Key_And_Cache_Expiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_message_consumers",
                schema: "general",
                table: "outbox_message_consumers");

            migrationBuilder.DropIndex(
                name: "ix_outbox_message_consumers_id_name",
                schema: "general",
                table: "outbox_message_consumers");

            migrationBuilder.AddColumn<DateTime>(
                name: "expires_on_utc",
                schema: "general",
                table: "cache_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_message_consumers",
                schema: "general",
                table: "outbox_message_consumers",
                columns: new[] { "id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_cache_items_expires_on_utc",
                schema: "general",
                table: "cache_items",
                column: "expires_on_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_message_consumers",
                schema: "general",
                table: "outbox_message_consumers");

            migrationBuilder.DropIndex(
                name: "ix_cache_items_expires_on_utc",
                schema: "general",
                table: "cache_items");

            migrationBuilder.DropColumn(
                name: "expires_on_utc",
                schema: "general",
                table: "cache_items");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_message_consumers",
                schema: "general",
                table: "outbox_message_consumers",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_consumers_id_name",
                schema: "general",
                table: "outbox_message_consumers",
                columns: new[] { "id", "name" },
                unique: true);
        }
    }
}
