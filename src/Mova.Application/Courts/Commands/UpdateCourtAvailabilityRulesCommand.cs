namespace Mova.Application.Courts.Commands;

public sealed record UpdateCourtAvailabilityRulesCommand(
    Guid SportsComplexId,
    Guid CourtId,
    IReadOnlyCollection<CourtAvailabilityRuleItem> Rules);

public sealed record CourtAvailabilityRuleItem(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int SlotDurationMinutes,
    bool IsActive);
