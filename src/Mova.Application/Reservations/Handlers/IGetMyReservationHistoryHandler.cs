using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IGetMyReservationHistoryHandler
{
    Task<PagedResult<ReservationInfo>> HandleAsync(GetMyReservationHistoryQuery query, CancellationToken cancellationToken = default);
}
