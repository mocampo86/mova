namespace Mova.Application.Reservations.Commands;

public sealed class CancelReservationCommand
{
    public CancelReservationCommand(
        Guid sportsComplexId,
        Guid reservationId,
        Guid cancelledByUserId,
        string? reason)
    {
        SportsComplexId = sportsComplexId;
        ReservationId = reservationId;
        CancelledByUserId = cancelledByUserId;
        Reason = reason;
    }

    public Guid SportsComplexId { get; }
    public Guid ReservationId { get; }
    public Guid CancelledByUserId { get; }
    public string? Reason { get; }
}
