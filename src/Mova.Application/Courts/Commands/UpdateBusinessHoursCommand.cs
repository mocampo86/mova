namespace Mova.Application.Courts.Commands;

public sealed record UpdateBusinessHoursCommand(
    Guid SportsComplexId,
    IReadOnlyCollection<BusinessHoursItem> Hours);

public sealed record BusinessHoursItem(
    DayOfWeek DayOfWeek,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime,
    bool IsClosed);
