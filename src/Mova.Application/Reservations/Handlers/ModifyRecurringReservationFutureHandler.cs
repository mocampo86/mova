using System.Data;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Handlers;

public sealed class ModifyRecurringReservationFutureHandler(
    IRecurringReservationRepository recurringReservations,
    IReservationRepository reservations,
    ICourtBlockRepository courtBlocks,
    IUnitOfWork unitOfWork) : IModifyRecurringReservationFutureHandler
{
    public async Task<RecurringReservationInfo> HandleAsync(ModifyRecurringReservationFutureCommand command, CancellationToken cancellationToken = default)
    {
        var recurringReservation = await recurringReservations.GetByIdAsync(command.RecurringReservationId, cancellationToken);
        if (recurringReservation is null ||
            recurringReservation.SportsComplexId != command.SportsComplexId ||
            recurringReservation.UserId != command.UserId)
        {
            throw new NotFoundException("Recurring reservation not found.");
        }

        if (recurringReservation.Status != RecurringReservationStatus.Active)
        {
            throw new ConflictException("Only active recurring reservations can be modified.");
        }

        return await unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                var effectiveFrom = command.EffectiveDate
                    .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                    .AddMinutes(command.UtcOffsetMinutes);
                var oldFutureOccurrences = await reservations.GetFutureActiveByRecurringReservationIdAsync(
                    command.RecurringReservationId,
                    effectiveFrom,
                    token);

                var newOccurrences = CreateRecurringReservationHandler.GenerateOccurrences(
                    recurringReservation.Id,
                    recurringReservation.SportsComplexId,
                    recurringReservation.CourtId,
                    recurringReservation.UserId,
                    command.DayOfWeek,
                    command.StartTime,
                    command.DurationMinutes,
                    command.EffectiveDate,
                    command.EndDate,
                    command.Notes,
                    command.UtcOffsetMinutes);

                if (newOccurrences.Count == 0)
                {
                    throw new ConflictException("The recurrence period does not contain any matching weekly occurrences.");
                }

                await EnsureAvailableAsync(recurringReservation.CourtId, newOccurrences, recurringReservation.Id, token);

                foreach (var occurrence in oldFutureOccurrences)
                {
                    occurrence.Cancel(command.UserId, false, "Modified recurring reservation.");
                }

                recurringReservation.ModifyFuture(
                    command.DayOfWeek,
                    command.StartTime,
                    command.DurationMinutes,
                    command.EffectiveDate,
                    command.EndDate);

                await reservations.AddRangeAsync(newOccurrences, token);
                await unitOfWork.SaveChangesAsync(token);

                return RecurringReservationMapper.ToInfo(recurringReservation, newOccurrences);
            },
            IsolationLevel.Serializable,
            cancellationToken);
    }

    private async Task EnsureAvailableAsync(Guid courtId, IReadOnlyList<Reservation> occurrences, Guid excludeRecurringReservationId, CancellationToken cancellationToken)
    {
        foreach (var occurrence in occurrences)
        {
            if (await reservations.HasOverlappingActiveReservationAsync(courtId, occurrence.StartAt, occurrence.EndAt, excludeRecurringReservationId: excludeRecurringReservationId, cancellationToken: cancellationToken))
            {
                throw new ConflictException($"The selected time is no longer available on {occurrence.StartAt:yyyy-MM-dd}.");
            }

            var blocks = await courtBlocks.GetForCourtAsync(courtId, occurrence.StartAt, occurrence.EndAt, cancellationToken);
            if (blocks.Count > 0)
            {
                throw new ConflictException($"The selected time is blocked on {occurrence.StartAt:yyyy-MM-dd}.");
            }
        }
    }
}
