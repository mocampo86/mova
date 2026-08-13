namespace Mova.Application.Reservations.Commands;

public sealed class CancelMyReservationCommand
{
    public CancelMyReservationCommand(Guid reservationId, Guid cancelledByUserId, string? reason)
    {
        ReservationId = reservationId;
        CancelledByUserId = cancelledByUserId;
        Reason = reason;
    }

    public Guid ReservationId { get; }

    public Guid CancelledByUserId { get; }

    public string? Reason { get; }
}
