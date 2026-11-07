using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Hash_Refresh_Tokens_And_Outbox_Permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Refresh tokens are now stored as SHA-256 hashes. Existing rows hold plain-text tokens that can't
            // be converted safely (and must not survive), so every session is invalidated: users sign in again.
            migrationBuilder.Sql("DELETE FROM users.refresh_tokens;");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_token",
                schema: "users",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_user_id",
                schema: "users",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "token",
                schema: "users",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<string>(
                name: "token_hash",
                schema: "users",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                schema: "users",
                table: "permissions",
                columns: new[] { "id", "name" },
                values: new object[] { 4, "ManageOutbox" });

            migrationBuilder.InsertData(
                schema: "users",
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[] { 4, 2 });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                schema: "users",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_expires_on_utc",
                schema: "users",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Hashes can't be turned back into tokens; drop the sessions instead.
            migrationBuilder.Sql("DELETE FROM users.refresh_tokens;");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_token_hash",
                schema: "users",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_user_id_expires_on_utc",
                schema: "users",
                table: "refresh_tokens");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 4, 2 });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "token_hash",
                schema: "users",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<string>(
                name: "token",
                schema: "users",
                table: "refresh_tokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token",
                schema: "users",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                schema: "users",
                table: "refresh_tokens",
                column: "user_id");
        }
    }
}
