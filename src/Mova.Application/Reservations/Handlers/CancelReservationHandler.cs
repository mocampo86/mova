using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public sealed class CancelReservationHandler(IReservationRepository reservations, IUnitOfWork unitOfWork) : ICancelReservationHandler
{
    public async Task<ReservationInfo?> HandleAsync(CancelReservationCommand command, CancellationToken cancellationToken = default)
    {
        var reservation = await reservations.GetByIdAsync(command.ReservationId, cancellationToken);

        if (reservation is null || reservation.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Reservation not found.");
        }

        reservation.Cancel(command.Reason, cancelledByAdmin: true);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToInfo(reservation);
    }
}
