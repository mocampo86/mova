using Mova.Application.Abstractions.Persistence;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public sealed class GetReservationByIdHandler(IReservationRepository reservations) : IGetReservationByIdHandler
{
    public async Task<ReservationInfo?> HandleAsync(GetReservationByIdQuery query, CancellationToken cancellationToken = default)
    {
        var reservation = await reservations.GetByIdAsync(query.ReservationId, cancellationToken);

        if (reservation is null || reservation.SportsComplexId != query.SportsComplexId)
        {
            return null;
        }

        return ReservationMapper.ToInfo(reservation);
    }
}
