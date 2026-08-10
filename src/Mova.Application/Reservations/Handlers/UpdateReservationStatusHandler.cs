using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Handlers;

public sealed class UpdateReservationStatusHandler(IReservationRepository reservations, IUnitOfWork unitOfWork) : IUpdateReservationStatusHandler
{
    public async Task<ReservationInfo?> HandleAsync(UpdateReservationStatusCommand command, CancellationToken cancellationToken = default)
    {
        var reservation = await reservations.GetByIdAsync(command.ReservationId, cancellationToken);

        if (reservation is null || reservation.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Reservation not found.");
        }

        try
        {
            switch (command.Status)
            {
                case ReservationStatus.Completed:
                    reservation.MarkCompleted();
                    break;
                case ReservationStatus.NoShow:
                    reservation.MarkNoShow();
                    break;
                default:
                    throw new ConflictException("Only Completed or NoShow statuses are supported.");
            }
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToInfo(reservation);
    }
}
