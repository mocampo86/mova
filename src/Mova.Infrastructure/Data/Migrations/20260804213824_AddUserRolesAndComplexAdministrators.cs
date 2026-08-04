using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRolesAndComplexAdministrators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Roles",
                table: "Users",
                type: "text[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::text[]");

            migrationBuilder.CreateTable(
                name: "ComplexAdministrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsComplexId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexAdministrators", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplexAdministrators_SportsComplexId_UserId",
                table: "ComplexAdministrators",
                columns: new[] { "SportsComplexId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplexAdministrators_UserId",
                table: "ComplexAdministrators",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplexAdministrators");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Users");
        }
    }
}
