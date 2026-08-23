using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourtAvailabilityDayOfWeekCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "chk_court_availability_day_of_week",
                table: "CourtAvailabilityRules",
                sql: "\"DayOfWeek\" BETWEEN 0 AND 6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_court_availability_day_of_week",
                table: "CourtAvailabilityRules");
        }
    }
}
