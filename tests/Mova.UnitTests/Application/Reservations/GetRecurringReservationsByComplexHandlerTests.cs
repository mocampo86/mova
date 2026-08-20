using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class GetRecurringReservationsByComplexHandlerTests
{
    private readonly FakeRecurringReservationRepository _recurringReservationRepository = new();

    private GetRecurringReservationsByComplexHandler CreateHandler() => new(_recurringReservationRepository);

    [Fact]
    public async Task HandleAsync_WithTwoRecurringReservations_ReturnsPagedResult()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var first = RecurringReservation.Create(
            complexId,
            courtId,
            userId,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var second = RecurringReservation.Create(
            complexId,
            courtId,
            userId,
            DayOfWeek.Tuesday,
            new TimeOnly(15, 0),
            90,
            new DateOnly(2030, 2, 1),
            new DateOnly(2030, 2, 15));

        await _recurringReservationRepository.AddAsync(first);
        await _recurringReservationRepository.AddAsync(second);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRecurringReservationsByComplexQuery(
            complexId,
            1,
            10));

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal(complexId, item.SportsComplexId));
    }

    [Fact]
    public async Task HandleAsync_WithStatusFilter_ReturnsMatchingRecurringReservations()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var active = RecurringReservation.Create(
            complexId,
            courtId,
            userId,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var cancelled = RecurringReservation.Create(
            complexId,
            courtId,
            userId,
            DayOfWeek.Tuesday,
            new TimeOnly(15, 0),
            90,
            new DateOnly(2030, 2, 1),
            new DateOnly(2030, 2, 15));
        cancelled.Cancel();

        await _recurringReservationRepository.AddAsync(active);
        await _recurringReservationRepository.AddAsync(cancelled);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRecurringReservationsByComplexQuery(
            complexId,
            1,
            10,
            status: "Cancelled"));

        Assert.Single(result.Items);
        Assert.Equal("Cancelled", result.Items[0].Status);
    }

    [Fact]
    public async Task HandleAsync_WithCourtFilter_ReturnsMatchingRecurringReservations()
    {
        var complexId = Guid.NewGuid();
        var courtA = Guid.NewGuid();
        var courtB = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var first = RecurringReservation.Create(
            complexId,
            courtA,
            userId,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var second = RecurringReservation.Create(
            complexId,
            courtB,
            userId,
            DayOfWeek.Tuesday,
            new TimeOnly(15, 0),
            90,
            new DateOnly(2030, 2, 1),
            new DateOnly(2030, 2, 15));

        await _recurringReservationRepository.AddAsync(first);
        await _recurringReservationRepository.AddAsync(second);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRecurringReservationsByComplexQuery(
            complexId,
            1,
            10,
            courtId: courtA));

        Assert.Single(result.Items);
        Assert.Equal(courtA, result.Items[0].CourtId);
    }

    [Fact]
    public async Task HandleAsync_WithUserFilter_ReturnsMatchingRecurringReservations()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var first = RecurringReservation.Create(
            complexId,
            courtId,
            userA,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var second = RecurringReservation.Create(
            complexId,
            courtId,
            userB,
            DayOfWeek.Tuesday,
            new TimeOnly(15, 0),
            90,
            new DateOnly(2030, 2, 1),
            new DateOnly(2030, 2, 15));

        await _recurringReservationRepository.AddAsync(first);
        await _recurringReservationRepository.AddAsync(second);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRecurringReservationsByComplexQuery(
            complexId,
            1,
            10,
            userId: userA));

        Assert.Single(result.Items);
        Assert.Equal(userA, result.Items[0].UserId);
    }

    [Fact]
    public async Task HandleAsync_WithPagination_ReturnsRequestedPage()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (var i = 0; i < 5; i++)
        {
            await _recurringReservationRepository.AddAsync(RecurringReservation.Create(
                complexId,
                courtId,
                userId,
                DayOfWeek.Monday,
                new TimeOnly(14, 0),
                60,
                new DateOnly(2030, 1, 7).AddDays(i * 7),
                new DateOnly(2030, 1, 21).AddDays(i * 7)));
        }

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRecurringReservationsByComplexQuery(
            complexId,
            1,
            2));

        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task HandleAsync_MapsListItemWithoutNavigationData()
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
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        await _recurringReservationRepository.AddAsync(series);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRecurringReservationsByComplexQuery(
            complexId,
            1,
            10));

        var item = Assert.Single(result.Items);
        Assert.Equal(series.Id, item.Id);
        Assert.Equal(series.CourtId, item.CourtId);
        Assert.Equal(series.UserId, item.UserId);
        Assert.Equal((int)DayOfWeek.Monday, item.DayOfWeek);
        Assert.Equal(series.StartTime, item.StartTime);
        Assert.Equal(series.DurationMinutes, item.DurationMinutes);
        Assert.Equal(series.StartDate, item.StartDate);
        Assert.Equal(series.EndDate, item.EndDate);
        Assert.Equal("Active", item.Status);
    }
}
