using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecurringReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsComplexId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringReservations", x => x.Id);
                    table.CheckConstraint("chk_recurring_dates", "\"StartDate\" <= \"EndDate\"");
                    table.ForeignKey(
                        name: "FK_RecurringReservations_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringReservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RecurringReservationId",
                table: "Reservations",
                column: "RecurringReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringReservations_CourtId_Status",
                table: "RecurringReservations",
                columns: new[] { "CourtId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringReservations_SportsComplexId",
                table: "RecurringReservations",
                column: "SportsComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringReservations_UserId",
                table: "RecurringReservations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_RecurringReservations_RecurringReservationId",
                table: "Reservations",
                column: "RecurringReservationId",
                principalTable: "RecurringReservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_RecurringReservations_RecurringReservationId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "RecurringReservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_RecurringReservationId",
                table: "Reservations");
        }
    }
}
