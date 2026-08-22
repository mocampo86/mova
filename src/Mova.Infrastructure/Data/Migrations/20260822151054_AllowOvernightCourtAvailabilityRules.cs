using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowOvernightCourtAvailabilityRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_court_availability_time",
                table: "CourtAvailabilityRules");

            migrationBuilder.AddCheckConstraint(
                name: "chk_court_availability_time",
                table: "CourtAvailabilityRules",
                sql: "\"StartTime\" <> \"EndTime\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_court_availability_time",
                table: "CourtAvailabilityRules");

            migrationBuilder.AddCheckConstraint(
                name: "chk_court_availability_time",
                table: "CourtAvailabilityRules",
                sql: "\"StartTime\" < \"EndTime\"");
        }
    }
}
