using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixBlockedUsersActiveFilteredIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BlockedUsers_SportsComplexId_UserId_Status",
                table: "BlockedUsers");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUsers_SportsComplexId_UserId_Status",
                table: "BlockedUsers",
                columns: new[] { "SportsComplexId", "UserId", "Status" },
                unique: true,
                filter: "\"Status\" = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BlockedUsers_SportsComplexId_UserId_Status",
                table: "BlockedUsers");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUsers_SportsComplexId_UserId_Status",
                table: "BlockedUsers",
                columns: new[] { "SportsComplexId", "UserId", "Status" },
                unique: true);
        }
    }
}
