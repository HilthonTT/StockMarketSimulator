using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Role_Users_Navigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_user_users_users_id",
                schema: "users",
                table: "role_user");

            migrationBuilder.RenameColumn(
                name: "users_id",
                schema: "users",
                table: "role_user",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "ix_role_user_users_id",
                schema: "users",
                table: "role_user",
                newName: "ix_role_user_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_role_user_users_user_id",
                schema: "users",
                table: "role_user",
                column: "user_id",
                principalSchema: "users",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_user_users_user_id",
                schema: "users",
                table: "role_user");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "users",
                table: "role_user",
                newName: "users_id");

            migrationBuilder.RenameIndex(
                name: "ix_role_user_user_id",
                schema: "users",
                table: "role_user",
                newName: "ix_role_user_users_id");

            migrationBuilder.AddForeignKey(
                name: "fk_role_user_users_users_id",
                schema: "users",
                table: "role_user",
                column: "users_id",
                principalSchema: "users",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
