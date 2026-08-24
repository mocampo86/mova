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

public sealed class CreateRecurringReservationHandler(
    ISportsComplexRepository sportsComplexes,
    ICourtRepository courts,
    IUserRepository users,
    IBlockedUserRepository blockedUsers,
    IRecurringReservationRepository recurringReservations,
    IReservationRepository reservations,
    ICourtBlockRepository courtBlocks,
    IUnitOfWork unitOfWork) : ICreateRecurringReservationHandler
{
    public async Task<RecurringReservationInfo> HandleAsync(CreateRecurringReservationCommand command, CancellationToken cancellationToken = default)
    {
        var complex = await ValidateContextAsync(command.SportsComplexId, command.CourtId, command.UserId, command.IsAdmin, cancellationToken);

        if (!TimeZoneConverter.TryGetTimeZone(complex.TimeZoneId, out var timeZone))
        {
            throw new UnresolvedTimeZoneException();
        }

        return await unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                var recurringReservation = RecurringReservation.Create(
                    command.SportsComplexId,
                    command.CourtId,
                    command.UserId,
                    command.DayOfWeek,
                    command.StartTime,
                    command.DurationMinutes,
                    command.StartDate,
                    command.EndDate);

                var occurrences = GenerateOccurrences(
                    recurringReservation.Id,
                    command.SportsComplexId,
                    command.CourtId,
                    command.UserId,
                    command.DayOfWeek,
                    command.StartTime,
                    command.DurationMinutes,
                    command.StartDate,
                    command.EndDate,
                    command.Notes,
                    timeZone);

                if (occurrences.Count == 0)
                {
                    throw new ConflictException("The recurrence period does not contain any matching weekly occurrences.");
                }

                await EnsureAvailableAsync(command.CourtId, occurrences, null, token);

                await recurringReservations.AddAsync(recurringReservation, token);
                await reservations.AddRangeAsync(occurrences, token);
                await unitOfWork.SaveChangesAsync(token);

                return RecurringReservationMapper.ToInfo(recurringReservation, occurrences);
            },
            IsolationLevel.Serializable,
            cancellationToken);
    }

    private async Task<SportsComplex> ValidateContextAsync(Guid sportsComplexId, Guid courtId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var complex = await sportsComplexes.GetByIdAsync(sportsComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        if (complex.Status != ComplexStatus.Active)
        {
            throw new ConflictException("The selected complex is not active.");
        }

        if (!isAdmin && !complex.AllowUserRecurringReservations)
        {
            throw new UserRecurringReservationsDisabledException("Recurring reservations are not available for this complex.");
        }

        var court = await courts.GetByIdAsync(courtId, cancellationToken);
        if (court is null || court.SportsComplexId != sportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        if (court.Status != CourtStatus.Active)
        {
            throw new ConflictException("The selected court is not active.");
        }

        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
        {
            throw new NotFoundException("User not found.");
        }

        if (await blockedUsers.IsUserBlockedAsync(sportsComplexId, userId, cancellationToken))
        {
            throw new UserBlockedException();
        }

        return complex;
    }

    private async Task EnsureAvailableAsync(Guid courtId, IReadOnlyList<Reservation> occurrences, Guid? excludeRecurringReservationId, CancellationToken cancellationToken)
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

    public static IReadOnlyList<Reservation> GenerateOccurrences(
        Guid recurringReservationId,
        Guid sportsComplexId,
        Guid courtId,
        Guid userId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        int durationMinutes,
        DateOnly startDate,
        DateOnly endDate,
        string? notes,
        TimeZoneInfo timeZone)
    {
        var firstDate = FirstDateOnOrAfter(startDate, dayOfWeek);
        var occurrences = new List<Reservation>();

        for (var date = firstDate; date <= endDate; date = date.AddDays(7))
        {
            var localStart = DateTime.SpecifyKind(date.ToDateTime(startTime), DateTimeKind.Unspecified);
            var localEnd = localStart.AddMinutes(durationMinutes);

            if (!TimeZoneConverter.TryGetUtc(localStart, timeZone, out var startAt) ||
                !TimeZoneConverter.TryGetUtc(localEnd, timeZone, out var endAt))
            {
                continue;
            }

            var reservation = Reservation.Create(
                sportsComplexId,
                courtId,
                userId,
                startAt,
                endAt,
                ReservationSource.Recurring,
                notes,
                recurringReservationId);

            reservation.Confirm();
            occurrences.Add(reservation);
        }

        return occurrences;
    }

    private static DateOnly FirstDateOnOrAfter(DateOnly date, DayOfWeek dayOfWeek)
    {
        var daysToAdd = ((int)dayOfWeek - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(daysToAdd);
    }
}
