using Mova.Application.Abstractions.Persistence;
using Mova.Application.Abstractions.Policies;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Handlers;

public sealed class CancelMyReservationHandler(
    IReservationRepository reservations,
    ICancellationPolicy cancellationPolicy,
    IUnitOfWork unitOfWork) : ICancelMyReservationHandler
{
    public async Task<ReservationInfo?> HandleAsync(CancelMyReservationCommand command, CancellationToken cancellationToken = default)
    {
        var reservation = await reservations.GetByIdAsync(command.ReservationId, cancellationToken);

        if (reservation is null || reservation.UserId != command.CancelledByUserId)
        {
            throw new NotFoundException("Reservation not found.");
        }

        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
        {
            throw new ConflictException("Only pending or confirmed reservations can be cancelled by the user.");
        }

        var evaluation = await cancellationPolicy.EvaluateAsync(
            reservation.SportsComplexId,
            reservation.StartAt,
            DateTime.UtcNow,
            cancellationToken);

        if (!evaluation.AllowUserCancellation)
        {
            throw new UserCancellationDisabledException("User cancellation is disabled for this complex.");
        }

        if (!evaluation.IsWithinWindow)
        {
            throw new CancellationDeadlineExceededException("The cancellation deadline has passed.");
        }

        reservation.Cancel(command.CancelledByUserId, false, command.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToInfo(reservation);
    }
}
