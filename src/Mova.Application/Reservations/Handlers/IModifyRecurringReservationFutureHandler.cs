using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IModifyRecurringReservationFutureHandler
{
    Task<RecurringReservationInfo> HandleAsync(ModifyRecurringReservationFutureCommand command, CancellationToken cancellationToken = default);
}
