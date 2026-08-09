using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IUpdateReservationStatusHandler
{
    Task<ReservationInfo?> HandleAsync(UpdateReservationStatusCommand command, CancellationToken cancellationToken = default);
}
