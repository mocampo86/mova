using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface ICancelRecurringReservationHandler
{
    Task<RecurringReservationInfo> HandleAsync(CancelRecurringReservationCommand command, CancellationToken cancellationToken = default);
}
