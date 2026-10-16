using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourtRatesAndReservationPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "SportsComplexes",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                defaultValue: "UYU");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Reservations",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourtRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    DaysOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    AmountPerHour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtRates", x => x.Id);
                    table.CheckConstraint("chk_court_rate_amount", "\"AmountPerHour\" > 0");
                    table.CheckConstraint("chk_court_rate_days_of_week", "\"DaysOfWeek\" BETWEEN 1 AND 127");
                    table.CheckConstraint("chk_court_rate_time", "\"StartTime\" >= interval '0 hours' AND \"StartTime\" < \"EndTime\" AND \"EndTime\" <= interval '24 hours'");
                    table.ForeignKey(
                        name: "FK_CourtRates_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservationPriceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtRateId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationPriceLines", x => x.Id);
                    table.CheckConstraint("chk_reservation_price_line_time", "\"StartAt\" < \"EndAt\"");
                    table.ForeignKey(
                        name: "FK_ReservationPriceLines_CourtRates_CourtRateId",
                        column: x => x.CourtRateId,
                        principalTable: "CourtRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReservationPriceLines_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourtRates_CourtId",
                table: "CourtRates",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationPriceLines_CourtRateId",
                table: "ReservationPriceLines",
                column: "CourtRateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationPriceLines_ReservationId",
                table: "ReservationPriceLines",
                column: "ReservationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationPriceLines");

            migrationBuilder.DropTable(
                name: "CourtRates");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "SportsComplexes");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Reservations");
        }
    }
}
