using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Commands;

public sealed class UpdateReservationStatusCommand
{
    public UpdateReservationStatusCommand(
        Guid sportsComplexId,
        Guid reservationId,
        ReservationStatus status)
    {
        SportsComplexId = sportsComplexId;
        ReservationId = reservationId;
        Status = status;
    }

    public Guid SportsComplexId { get; }
    public Guid ReservationId { get; }
    public ReservationStatus Status { get; }
}
