using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceUtcOffsetWithTimeZoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UtcOffsetMinutes",
                table: "SportsComplexes");

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "SportsComplexes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "SportsComplexes");

            migrationBuilder.AddColumn<int>(
                name: "UtcOffsetMinutes",
                table: "SportsComplexes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
