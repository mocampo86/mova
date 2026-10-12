using Mova.Contracts.CourtRates;
using Mova.Domain.Entities;

namespace Mova.Application.CourtRates;

public static class CourtRateMapper
{
    public static CourtRateInfo ToInfo(CourtRate rate) => new()
    {
        Id = rate.Id,
        CourtId = rate.CourtId,
        DaysOfWeek = Enumerable.Range(0, 7)
            .Where(d => rate.AppliesOn((DayOfWeek)d))
            .ToArray(),
        StartTime = rate.StartTime,
        EndTime = rate.EndTime,
        AmountPerHour = rate.AmountPerHour,
        IsActive = rate.IsActive,
        CreatedAt = rate.CreatedAt,
        UpdatedAt = rate.UpdatedAt
    };

    public static int ToDaysMask(IReadOnlyCollection<DayOfWeek> daysOfWeek) =>
        daysOfWeek.Distinct().Aggregate(0, (mask, day) => mask | (1 << (int)day));
}
