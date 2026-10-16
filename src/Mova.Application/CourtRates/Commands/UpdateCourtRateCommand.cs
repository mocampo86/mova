namespace Mova.Application.CourtRates.Commands;

public sealed record UpdateCourtRateCommand(
    Guid SportsComplexId,
    Guid CourtId,
    Guid RateId,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal AmountPerHour);
