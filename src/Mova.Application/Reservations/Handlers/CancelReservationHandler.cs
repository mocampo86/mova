using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;

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

        if (reservation.Status is ReservationStatus.Completed or ReservationStatus.NoShow)
        {
            throw new ConflictException("Only pending or confirmed reservations can be cancelled.");
        }

        reservation.Cancel(command.CancelledByUserId, true, command.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToInfo(reservation);
    }
}
