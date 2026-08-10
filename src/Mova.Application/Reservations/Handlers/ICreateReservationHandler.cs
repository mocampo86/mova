using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface ICreateReservationHandler
{
    Task<ReservationInfo> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken = default);
}
