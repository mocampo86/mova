using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IGetRecurringReservationsByComplexHandler
{
    Task<PagedResult<RecurringReservationListItem>> HandleAsync(GetRecurringReservationsByComplexQuery query, CancellationToken cancellationToken = default);
}
