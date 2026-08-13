using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class GetComplexDashboardHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeCourtRepository _courtRepository = new();
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();

    private GetComplexDashboardHandler CreateHandler() =>
        new(_sportsComplexRepository, _courtRepository, _reservationRepository, _blockedUserRepository);

    [Fact]
    public async Task HandleAsync_WithExistingComplex_ReturnsAggregatedDashboard()
    {
        var complex = SportsComplex.Create(
            "Dashboard Club",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "dashboard@test.com");
        await _sportsComplexRepository.AddAsync(complex);

        var activeCourt = Court.Create(complex.Id, "Court 1", "Court", "Synthetic", true);
        var inactiveCourt = Court.Create(complex.Id, "Court 2", "Court", "Grass", false);
        inactiveCourt.Deactivate();
        await _courtRepository.AddAsync(activeCourt);
        await _courtRepository.AddAsync(inactiveCourt);

        var today = DateTime.UtcNow.Date;
        var start = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);

        await _reservationRepository.AddAsync(CreateReservation(complex.Id, activeCourt.Id, ReservationStatus.Confirmed, start.AddHours(10)));
        await _reservationRepository.AddAsync(CreateReservation(complex.Id, activeCourt.Id, ReservationStatus.Confirmed, start.AddHours(11)));
        await _reservationRepository.AddAsync(CreateReservation(complex.Id, activeCourt.Id, ReservationStatus.CancelledByUser, start.AddHours(12)));
        await _reservationRepository.AddAsync(CreateReservation(complex.Id, activeCourt.Id, ReservationStatus.Completed, start.AddHours(13)));

        await _blockedUserRepository.AddAsync(BlockedUser.Create(complex.Id, Guid.NewGuid(), Guid.NewGuid()));
        await _blockedUserRepository.AddAsync(BlockedUser.Create(complex.Id, Guid.NewGuid(), Guid.NewGuid()));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetComplexDashboardQuery(complex.Id));

        Assert.NotNull(result);
        Assert.Equal(complex.Id, result.Complex.Id);
        Assert.Equal(complex.Name, result.Complex.Name);
        Assert.Equal(1, result.Courts.Active);
        Assert.Equal(1, result.Courts.Inactive);
        Assert.Equal(2, result.ReservationsToday.Confirmed);
        Assert.Equal(1, result.ReservationsToday.Cancelled);
        Assert.Equal(1, result.ReservationsToday.Completed);
        Assert.Equal(2, result.BlockedUsers);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ReturnsNull()
    {
        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetComplexDashboardQuery(Guid.NewGuid()));

        Assert.Null(result);
    }

    private static Reservation CreateReservation(Guid complexId, Guid courtId, ReservationStatus status, DateTime startAt)
    {
        var reservation = Reservation.Create(
            complexId,
            courtId,
            Guid.NewGuid(),
            startAt,
            startAt.AddHours(1),
            ReservationSource.Web);

        if (status == ReservationStatus.Confirmed)
        {
            reservation.Confirm();
        }
        else if (status is ReservationStatus.CancelledByUser or ReservationStatus.CancelledByAdmin)
        {
            reservation.Cancel(Guid.NewGuid());
        }
        else if (status == ReservationStatus.Completed)
        {
            reservation.Confirm();
            // Completed is a terminal status set by an admin process; setting via reflection for tests.
            typeof(Reservation).GetProperty("Status")!.SetValue(reservation, ReservationStatus.Completed);
        }

        return reservation;
    }
}
