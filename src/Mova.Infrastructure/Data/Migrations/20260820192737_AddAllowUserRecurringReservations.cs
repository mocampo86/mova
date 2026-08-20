using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowUserRecurringReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowUserRecurringReservations",
                table: "SportsComplexes",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowUserRecurringReservations",
                table: "SportsComplexes");
        }
    }
}
