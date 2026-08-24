using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixEPIC05_BusinessHoursAndTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UtcOffsetMinutes",
                table: "SportsComplexes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "chk_business_hours_closing_time",
                table: "BusinessHours",
                sql: "\"ClosingTime\" >= '00:00:00' AND \"ClosingTime\" < '24:00:00'");

            migrationBuilder.AddCheckConstraint(
                name: "chk_business_hours_day_of_week",
                table: "BusinessHours",
                sql: "\"DayOfWeek\" BETWEEN 0 AND 6");

            migrationBuilder.AddCheckConstraint(
                name: "chk_business_hours_not_closed",
                table: "BusinessHours",
                sql: "\"IsClosed\" = TRUE OR \"OpeningTime\" <> \"ClosingTime\"");

            migrationBuilder.AddCheckConstraint(
                name: "chk_business_hours_opening_time",
                table: "BusinessHours",
                sql: "\"OpeningTime\" >= '00:00:00' AND \"OpeningTime\" < '24:00:00'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_business_hours_closing_time",
                table: "BusinessHours");

            migrationBuilder.DropCheckConstraint(
                name: "chk_business_hours_day_of_week",
                table: "BusinessHours");

            migrationBuilder.DropCheckConstraint(
                name: "chk_business_hours_not_closed",
                table: "BusinessHours");

            migrationBuilder.DropCheckConstraint(
                name: "chk_business_hours_opening_time",
                table: "BusinessHours");

            migrationBuilder.DropColumn(
                name: "UtcOffsetMinutes",
                table: "SportsComplexes");
        }
    }
}
