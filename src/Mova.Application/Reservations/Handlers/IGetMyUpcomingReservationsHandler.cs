using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IGetMyUpcomingReservationsHandler
{
    Task<PagedResult<ReservationInfo>> HandleAsync(GetMyUpcomingReservationsQuery query, CancellationToken cancellationToken = default);
}
