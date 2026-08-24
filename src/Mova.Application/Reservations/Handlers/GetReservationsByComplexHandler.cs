using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;
using Mova.Domain.Exceptions;
using Mova.Domain.Helpers;

namespace Mova.Application.Reservations.Handlers;

public sealed class GetReservationsByComplexHandler(
    IReservationRepository reservations,
    ISportsComplexRepository sportsComplexes) : IGetReservationsByComplexHandler
{
    public async Task<PagedResult<ReservationInfo>> HandleAsync(GetReservationsByComplexQuery query, CancellationToken cancellationToken = default)
    {
        ReservationStatus? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<ReservationStatus>(query.Status, true, out var parsed))
            {
                statusFilter = parsed;
            }
        }

        DateTime? dayStart = null;
        DateTime? dayEnd = null;

        if (query.Date.HasValue)
        {
            var complex = await sportsComplexes.GetByIdAsync(query.SportsComplexId, cancellationToken)
                ?? throw new NotFoundException("Sports complex not found.");

            if (!TimeZoneConverter.TryGetTimeZone(complex.TimeZoneId, out var timeZone))
            {
                throw new UnresolvedTimeZoneException();
            }

            dayStart = TimeZoneConverter.GetDayStartUtc(query.Date.Value, timeZone);
            dayEnd = TimeZoneConverter.GetDayStartUtc(query.Date.Value.AddDays(1), timeZone);
        }

        var (items, totalItems) = await reservations.GetByComplexIdAsync(
            query.SportsComplexId,
            query.Page,
            query.PageSize,
            query.CourtId,
            statusFilter,
            dayStart,
            dayEnd,
            query.Sort,
            query.UserId,
            cancellationToken);

        var mappedItems = items.Select(ReservationMapper.ToInfo).ToList();

        return PagedResult<ReservationInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
