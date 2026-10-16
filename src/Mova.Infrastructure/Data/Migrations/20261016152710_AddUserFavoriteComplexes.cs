using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFavoriteComplexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavoriteComplexes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsComplexId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteComplexes", x => new { x.UserId, x.SportsComplexId });
                    table.ForeignKey(
                        name: "FK_UserFavoriteComplexes_SportsComplexes_SportsComplexId",
                        column: x => x.SportsComplexId,
                        principalTable: "SportsComplexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteComplexes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteComplexes_SportsComplexId",
                table: "UserFavoriteComplexes",
                column: "SportsComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteComplexes_UserId_CreatedAt",
                table: "UserFavoriteComplexes",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteComplexes");
        }
    }
}
