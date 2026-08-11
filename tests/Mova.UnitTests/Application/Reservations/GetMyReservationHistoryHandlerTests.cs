using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class GetMyReservationHistoryHandlerTests
{
    private readonly FakeReservationRepository _reservationRepository = new();

    private GetMyReservationHistoryHandler CreateHandler() =>
        new(_reservationRepository);

    [Fact]
    public async Task HandleAsync_WithHistory_ReturnsPastAndCompletedReservations()
    {
        var userId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var pastReservation = Reservation.Create(complexId, courtId, userId, now.AddHours(-2), now.AddHours(-1), ReservationSource.Web);
        var completedReservation = Reservation.Create(complexId, courtId, userId, now.AddHours(1), now.AddHours(2), ReservationSource.Web);
        completedReservation.MarkCompleted();

        await _reservationRepository.AddAsync(pastReservation);
        await _reservationRepository.AddAsync(completedReservation);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetMyReservationHistoryQuery(userId, 1, 10));

        Assert.Equal(2, result.TotalItems);
        Assert.Contains(result.Items, r => r.Id == pastReservation.Id);
        Assert.Contains(result.Items, r => r.Id == completedReservation.Id);
    }

    [Fact]
    public async Task HandleAsync_WithUpcomingOnly_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var upcomingReservation = Reservation.Create(complexId, courtId, userId, now.AddHours(1), now.AddHours(2), ReservationSource.Web);
        await _reservationRepository.AddAsync(upcomingReservation);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetMyReservationHistoryQuery(userId, 1, 10));

        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items);
    }
}
