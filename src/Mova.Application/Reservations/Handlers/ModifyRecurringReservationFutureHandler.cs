using System.Data;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Exceptions;
using Mova.Domain.Helpers;

namespace Mova.Application.Reservations.Handlers;

public sealed class ModifyRecurringReservationFutureHandler(
    IRecurringReservationRepository recurringReservations,
    IReservationRepository reservations,
    ICourtBlockRepository courtBlocks,
    ISportsComplexRepository sportsComplexes,
    IAuditLogRepository auditLogs,
    IUnitOfWork unitOfWork) : IModifyRecurringReservationFutureHandler
{
    public async Task<RecurringReservationInfo> HandleAsync(ModifyRecurringReservationFutureCommand command, CancellationToken cancellationToken = default)
    {
        var recurringReservation = await recurringReservations.GetByIdAsync(command.RecurringReservationId, cancellationToken);
        if (recurringReservation is null ||
            recurringReservation.SportsComplexId != command.SportsComplexId ||
            (recurringReservation.UserId != command.UserId && !command.IsAdmin))
        {
            throw new NotFoundException("Recurring reservation not found.");
        }

        if (recurringReservation.Status != RecurringReservationStatus.Active)
        {
            throw new ConflictException("Only active recurring reservations can be modified.");
        }

        var complex = await sportsComplexes.GetByIdAsync(command.SportsComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        if (!TimeZoneConverter.TryGetTimeZone(complex.TimeZoneId, out var timeZone))
        {
            throw new UnresolvedTimeZoneException();
        }

        return await unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                var effectiveFrom = TimeZoneConverter.GetDayStartUtc(command.EffectiveDate, timeZone);
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
                    timeZone);

                if (newOccurrences.Count == 0)
                {
                    throw new ConflictException("The recurrence period does not contain any matching weekly occurrences.");
                }

                await EnsureAvailableAsync(recurringReservation.CourtId, newOccurrences, recurringReservation.Id, token);

                var cancelledByAdmin = command.IsAdmin && recurringReservation.UserId != command.UserId;

                foreach (var occurrence in oldFutureOccurrences)
                {
                    occurrence.Cancel(command.UserId, cancelledByAdmin, "Modified recurring reservation.");
                }

                recurringReservation.ModifyFuture(
                    command.DayOfWeek,
                    command.StartTime,
                    command.DurationMinutes,
                    command.EffectiveDate,
                    command.EndDate);

                await reservations.AddRangeAsync(newOccurrences, token);

                if (command.IsAdmin)
                {
                    var auditLog = AuditLog.Create(
                        command.UserId,
                        command.SportsComplexId,
                        "RecurringReservation.ModifyFuture",
                        "RecurringReservation",
                        recurringReservation.Id.ToString(),
                        new { effectiveDate = command.EffectiveDate, dayOfWeek = command.DayOfWeek.ToString(), startTime = command.StartTime, durationMinutes = command.DurationMinutes, endDate = command.EndDate, occurrenceCount = newOccurrences.Count });

                    await auditLogs.AddAsync(auditLog, token);
                }

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
