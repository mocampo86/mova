using Mova.Application.Abstractions.Persistence;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public sealed class GetMyReservationHistoryHandler(IReservationRepository reservations) : IGetMyReservationHistoryHandler
{
    public async Task<PagedResult<ReservationInfo>> HandleAsync(GetMyReservationHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await reservations.GetHistoryByUserIdAsync(
            query.UserId,
            DateTime.UtcNow,
            query.Page,
            query.PageSize,
            cancellationToken);

        var mappedItems = items.Select(ReservationMapper.ToInfo).ToList();

        return PagedResult<ReservationInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }
}
