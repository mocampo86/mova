using Mova.Application.Reservations.Queries;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IGetReservationByIdHandler
{
    Task<ReservationInfo?> HandleAsync(GetReservationByIdQuery query, CancellationToken cancellationToken = default);
}
