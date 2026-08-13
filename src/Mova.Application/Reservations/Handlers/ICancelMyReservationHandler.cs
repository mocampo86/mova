using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface ICancelMyReservationHandler
{
    Task<ReservationInfo?> HandleAsync(CancelMyReservationCommand command, CancellationToken cancellationToken = default);
}
