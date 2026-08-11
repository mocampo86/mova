using Mova.Application.Abstractions.Persistence;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public sealed class GetMyUpcomingReservationsHandler(IReservationRepository reservations) : IGetMyUpcomingReservationsHandler
{
    public async Task<PagedResult<ReservationInfo>> HandleAsync(GetMyUpcomingReservationsQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await reservations.GetUpcomingByUserIdAsync(
            query.UserId,
            query.From,
            query.Page,
            query.PageSize,
            cancellationToken);

        var mappedItems = items.Select(ReservationMapper.ToInfo).ToList();

        return PagedResult<ReservationInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
