using Mova.Application.Abstractions.Persistence;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Handlers;

public sealed class GetRecurringReservationsByComplexHandler(IRecurringReservationRepository recurringReservations) : IGetRecurringReservationsByComplexHandler
{
    public async Task<PagedResult<RecurringReservationListItem>> HandleAsync(GetRecurringReservationsByComplexQuery query, CancellationToken cancellationToken = default)
    {
        RecurringReservationStatus? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<RecurringReservationStatus>(query.Status, true, out var parsed))
            {
                statusFilter = parsed;
            }
        }

        var (items, totalItems) = await recurringReservations.GetByComplexIdAsync(
            query.SportsComplexId,
            query.Page,
            query.PageSize,
            query.UserId,
            query.CourtId,
            statusFilter,
            query.Sort,
            cancellationToken);

        var mappedItems = items.Select(RecurringReservationMapper.ToListItem).ToList();

        return PagedResult<RecurringReservationListItem>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
