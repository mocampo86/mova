using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IGetReservationsByComplexHandler
{
    Task<PagedResult<ReservationInfo>> HandleAsync(GetReservationsByComplexQuery query, CancellationToken cancellationToken = default);
}
