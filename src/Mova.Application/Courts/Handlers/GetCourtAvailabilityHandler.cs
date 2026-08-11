using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts.Handlers;

public sealed class GetCourtAvailabilityHandler(
    ICourtRepository courts,
    ICourtAvailabilityRuleRepository rules,
    IBusinessHoursRepository businessHours,
    IReservationRepository reservations,
    ICourtBlockRepository blocks) : IGetCourtAvailabilityHandler
{
    public async Task<IReadOnlyCollection<CourtAvailabilitySlotInfo>> HandleAsync(GetCourtAvailabilityQuery query, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetActiveByIdAsync(query.CourtId, cancellationToken);

        if (court is null || court.SportsComplexId != query.SportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        var dayOfWeek = query.Date.DayOfWeek;
        var rulesForCourt = await rules.GetByCourtIdAsync(query.CourtId, cancellationToken);
        var rule = rulesForCourt.FirstOrDefault(r => r.DayOfWeek == dayOfWeek && r.IsActive)
                   ?? (rulesForCourt.Any() ? null : CourtAvailabilityRule.Create(query.CourtId, dayOfWeek, TimeSpan.FromHours(8), TimeSpan.FromHours(22), 60, true));

        if (rule is null)
        {
            return [];
        }

        var hoursForComplex = await businessHours.GetBySportsComplexIdAsync(query.SportsComplexId, cancellationToken);
        var businessHour = hoursForComplex.FirstOrDefault(h => h.DayOfWeek == dayOfWeek)
                          ?? (hoursForComplex.Any() ? null : BusinessHours.Create(query.SportsComplexId, dayOfWeek, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false));

        if (businessHour is null)
        {
            return [];
        }

        var dayStart = query.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var queryEnd = dayStart.AddDays(2);

        var courtReservations = await reservations.GetActiveForCourtAsync(query.CourtId, dayStart, queryEnd, cancellationToken);
        var courtBlocks = await blocks.GetForCourtAsync(query.CourtId, dayStart, queryEnd, cancellationToken);

        return AvailabilitySlotGenerator.GenerateSlots(query.CourtId, query.Date, rule, businessHour, courtReservations, courtBlocks);
    }
}
