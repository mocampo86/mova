using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface ICreateRecurringReservationHandler
{
    Task<RecurringReservationInfo> HandleAsync(CreateRecurringReservationCommand command, CancellationToken cancellationToken = default);
}
