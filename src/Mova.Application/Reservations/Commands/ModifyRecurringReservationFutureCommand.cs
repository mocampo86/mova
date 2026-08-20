namespace Mova.Application.Reservations.Commands;

public sealed record ModifyRecurringReservationFutureCommand(
    Guid SportsComplexId,
    Guid RecurringReservationId,
    Guid UserId,
    DateOnly EffectiveDate,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    DateOnly EndDate,
    string? Notes,
    int UtcOffsetMinutes = 0);
