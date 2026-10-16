namespace Mova.Application.CourtRates.Commands;

public sealed record CreateCourtRateCommand(
    Guid SportsComplexId,
    Guid CourtId,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal AmountPerHour);
