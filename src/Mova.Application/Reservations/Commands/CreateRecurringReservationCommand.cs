namespace Mova.Application.Reservations.Commands;

public sealed record CreateRecurringReservationCommand(
    Guid SportsComplexId,
    Guid CourtId,
    Guid UserId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    bool IsAdmin);
