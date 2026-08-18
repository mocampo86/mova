using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class RecurringReservationMutationHandlerTests
{
    private readonly FakeRecurringReservationRepository _recurringReservationRepository = new();
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeCourtBlockRepository _courtBlockRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task CancelAsync_WithActiveSeries_CancelsFutureOccurrences()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var handler = new CancelRecurringReservationHandler(
            _recurringReservationRepository,
            _reservationRepository,
            new FakeCancellationPolicy(0, true),
            _unitOfWork);

        var result = await handler.HandleAsync(new CancelRecurringReservationCommand(
            series.SportsComplexId,
            series.Id,
            series.UserId,
            "No longer needed"));

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(3, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence => Assert.Equal("CancelledByUser", occurrence.Status));
    }

    [Fact]
    public async Task ModifyFutureAsync_CancelsOldFutureAndCreatesNewOccurrences()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 24));
        var handler = new ModifyRecurringReservationFutureHandler(
            _recurringReservationRepository,
            _reservationRepository,
            _courtBlockRepository,
            _unitOfWork);

        var result = await handler.HandleAsync(new ModifyRecurringReservationFutureCommand(
            series.SportsComplexId,
            series.Id,
            series.UserId,
            new DateOnly(2026, 8, 17),
            DayOfWeek.Tuesday,
            new TimeOnly(16, 0),
            90,
            new DateOnly(2026, 8, 31),
            "Moved"));

        Assert.Equal(DayOfWeek.Tuesday, (DayOfWeek)result.DayOfWeek);
        Assert.Equal(new TimeOnly(16, 0), result.StartTime);
        Assert.Equal(2, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence =>
        {
            Assert.Equal("Confirmed", occurrence.Status);
            Assert.Equal("Recurring", occurrence.Source);
            Assert.Equal(TimeOnly.FromDateTime(occurrence.StartAt), new TimeOnly(16, 0));
        });

        var oldFuture = await _reservationRepository.GetFutureActiveByRecurringReservationIdAsync(
            series.Id,
            new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(2, oldFuture.Count);
    }

    private async Task<RecurringReservation> CreateSeriesWithOccurrencesAsync(DateOnly startDate, DateOnly endDate)
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var series = RecurringReservation.Create(
            complexId,
            courtId,
            userId,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            startDate,
            endDate);

        await _recurringReservationRepository.AddAsync(series);

        var occurrences = CreateRecurringReservationHandler.GenerateOccurrences(
            series.Id,
            complexId,
            courtId,
            userId,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            startDate,
            endDate,
            null);

        await _reservationRepository.AddRangeAsync(occurrences);
        return series;
    }
}
