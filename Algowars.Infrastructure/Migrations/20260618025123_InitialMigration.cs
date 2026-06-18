using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algowars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    username_last_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_sub",
                table: "users",
                column: "sub",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
