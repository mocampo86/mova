using Mova.Application.Abstractions.Persistence;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Handlers;

public sealed class GetReservationsByComplexHandler(IReservationRepository reservations) : IGetReservationsByComplexHandler
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

        var (items, totalItems) = await reservations.GetByComplexIdAsync(
            query.SportsComplexId,
            query.Page,
            query.PageSize,
            query.CourtId,
            statusFilter,
            query.Date,
            query.Sort,
            query.UserId,
            cancellationToken);

        var mappedItems = items.Select(ReservationMapper.ToInfo).ToList();

        return PagedResult<ReservationInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
