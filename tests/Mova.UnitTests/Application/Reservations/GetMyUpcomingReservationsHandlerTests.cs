using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class GetMyUpcomingReservationsHandlerTests
{
    private readonly FakeReservationRepository _reservationRepository = new();

    private GetMyUpcomingReservationsHandler CreateHandler() => new(_reservationRepository);

    [Fact]
    public async Task HandleAsync_ReturnsOnlyCallerUpcomingActiveReservationsOrderedByStart()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var now = new DateTime(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);

        await _reservationRepository.AddAsync(Reservation.Create(complexId, courtId, userId, now.AddDays(-1), now.AddDays(-1).AddHours(1), ReservationSource.Web));
        await _reservationRepository.AddAsync(Reservation.Create(complexId, courtId, otherUserId, now.AddHours(1), now.AddHours(2), ReservationSource.Web));

        var later = Reservation.Create(complexId, courtId, userId, now.AddHours(4), now.AddHours(5), ReservationSource.Web);
        var sooner = Reservation.Create(complexId, courtId, userId, now.AddHours(1), now.AddHours(2), ReservationSource.Web);
        await _reservationRepository.AddAsync(later);
        await _reservationRepository.AddAsync(sooner);

        var cancelled = Reservation.Create(complexId, courtId, userId, now.AddHours(2), now.AddHours(3), ReservationSource.Web);
        cancelled.Cancel("Changed plans");
        await _reservationRepository.AddAsync(cancelled);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetMyUpcomingReservationsQuery(userId, now, 1, 10));

        Assert.Equal(2, result.TotalItems);
        Assert.Equal([sooner.Id, later.Id], result.Items.Select(r => r.Id));
    }
}
