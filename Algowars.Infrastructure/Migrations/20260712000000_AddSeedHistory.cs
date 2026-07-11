using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Algowars.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSeedHistory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "seed_history",
            columns: table => new
            {
                name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_seed_history", x => x.name);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "seed_history");
    }
}
