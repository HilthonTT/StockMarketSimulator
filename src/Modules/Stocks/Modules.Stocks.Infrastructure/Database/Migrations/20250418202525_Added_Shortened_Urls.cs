using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Stocks.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Added_Shortened_Urls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shortened_urls",
                schema: "stocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    short_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    original_url = table.Column<string>(type: "text", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shortened_urls", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shortened_urls_short_code",
                schema: "stocks",
                table: "shortened_urls",
                column: "short_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shortened_urls",
                schema: "stocks");
        }
    }
}
