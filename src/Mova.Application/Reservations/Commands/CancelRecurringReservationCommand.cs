namespace Mova.Application.Reservations.Commands;

public sealed record CancelRecurringReservationCommand(
    Guid SportsComplexId,
    Guid RecurringReservationId,
    Guid CancelledByUserId,
    string? Reason);
