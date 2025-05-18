using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Followers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_public_profile",
                schema: "users",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "followers",
                schema: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    followed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_followers", x => new { x.user_id, x.followed_id });
                    table.ForeignKey(
                        name: "fk_followers_users_followed_id",
                        column: x => x.followed_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_followers_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_followers_followed_id",
                schema: "users",
                table: "followers",
                column: "followed_id");

            migrationBuilder.CreateIndex(
                name: "ix_followers_user_id_followed_id",
                schema: "users",
                table: "followers",
                columns: new[] { "user_id", "followed_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "followers",
                schema: "users");

            migrationBuilder.DropColumn(
                name: "has_public_profile",
                schema: "users",
                table: "users");
        }
    }
}
