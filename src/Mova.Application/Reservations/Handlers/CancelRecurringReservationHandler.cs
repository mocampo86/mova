using Mova.Application.Abstractions.Persistence;
using Mova.Application.Abstractions.Policies;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Handlers;

public sealed class CancelRecurringReservationHandler(
    IRecurringReservationRepository recurringReservations,
    IReservationRepository reservations,
    ICancellationPolicy cancellationPolicy,
    IUnitOfWork unitOfWork) : ICancelRecurringReservationHandler
{
    public async Task<RecurringReservationInfo> HandleAsync(CancelRecurringReservationCommand command, CancellationToken cancellationToken = default)
    {
        var recurringReservation = await recurringReservations.GetByIdAsync(command.RecurringReservationId, cancellationToken);
        if (recurringReservation is null ||
            recurringReservation.SportsComplexId != command.SportsComplexId ||
            recurringReservation.UserId != command.CancelledByUserId)
        {
            throw new NotFoundException("Recurring reservation not found.");
        }

        if (recurringReservation.Status != RecurringReservationStatus.Active)
        {
            throw new ConflictException("Only active recurring reservations can be cancelled.");
        }

        var futureOccurrences = await reservations.GetFutureActiveByRecurringReservationIdAsync(
            command.RecurringReservationId,
            DateTime.UtcNow,
            cancellationToken);

        foreach (var occurrence in futureOccurrences)
        {
            var evaluation = await cancellationPolicy.EvaluateAsync(
                occurrence.SportsComplexId,
                occurrence.StartAt,
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
        }

        recurringReservation.Cancel();

        foreach (var occurrence in futureOccurrences)
        {
            occurrence.Cancel(command.CancelledByUserId, false, command.Reason);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RecurringReservationMapper.ToInfo(recurringReservation, futureOccurrences);
    }
}
