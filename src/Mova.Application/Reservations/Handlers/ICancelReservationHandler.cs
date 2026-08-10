using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface ICancelReservationHandler
{
    Task<ReservationInfo?> HandleAsync(CancelReservationCommand command, CancellationToken cancellationToken = default);
}
