namespace Mova.Application.Complexes.Commands;

public sealed record UpdateRecurringReservationSettingsCommand(
    Guid ComplexId,
    bool AllowUserRecurringReservations);
