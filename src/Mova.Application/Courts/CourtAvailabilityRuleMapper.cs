using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts;

public static class CourtAvailabilityRuleMapper
{
    public static CourtAvailabilityRuleInfo ToInfo(CourtAvailabilityRule rule) => new()
    {
        Id = rule.Id,
        CourtId = rule.CourtId,
        DayOfWeek = rule.DayOfWeek,
        StartTime = rule.StartTime,
        EndTime = rule.EndTime,
        SlotDurationMinutes = rule.SlotDurationMinutes,
        IsActive = rule.IsActive
    };
}
