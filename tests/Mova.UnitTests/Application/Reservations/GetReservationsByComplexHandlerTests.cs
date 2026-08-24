using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class GetReservationsByComplexHandlerTests
{
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();

    private GetReservationsByComplexHandler CreateHandler() => new(_reservationRepository, _sportsComplexRepository);

    [Fact]
    public async Task HandleAsync_WithTwoReservations_ReturnsPagedResult()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var start = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);

        var first = Reservation.Create(complexId, courtId, userId, start, start.AddHours(1), ReservationSource.Web);
        var second = Reservation.Create(complexId, courtId, userId, start.AddHours(2), start.AddHours(3), ReservationSource.Web);
        await _reservationRepository.AddAsync(first);
        await _reservationRepository.AddAsync(second);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetReservationsByComplexQuery(
            complexId,
            1,
            10));

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task HandleAsync_WithStatusFilter_ReturnsMatchingReservation()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var start = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);

        var confirmed = Reservation.Create(complexId, courtId, userId, start, start.AddHours(1), ReservationSource.Web);
        confirmed.Confirm();
        var pending = Reservation.Create(complexId, courtId, userId, start.AddHours(2), start.AddHours(3), ReservationSource.Web);
        await _reservationRepository.AddAsync(confirmed);
        await _reservationRepository.AddAsync(pending);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetReservationsByComplexQuery(
            complexId,
            1,
            10,
            status: "Confirmed"));

        Assert.Single(result.Items);
        Assert.Equal("Confirmed", result.Items[0].Status);
    }

    [Fact]
    public async Task HandleAsync_WithCourtFilter_ReturnsMatchingReservation()
    {
        var complexId = Guid.NewGuid();
        var courtA = Guid.NewGuid();
        var courtB = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var start = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);

        await _reservationRepository.AddAsync(Reservation.Create(complexId, courtA, userId, start, start.AddHours(1), ReservationSource.Web));
        await _reservationRepository.AddAsync(Reservation.Create(complexId, courtB, userId, start.AddHours(2), start.AddHours(3), ReservationSource.Web));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetReservationsByComplexQuery(
            complexId,
            1,
            10,
            courtId: courtA));

        Assert.Single(result.Items);
        Assert.Equal(courtA, result.Items[0].CourtId);
    }

    [Fact]
    public async Task HandleAsync_WithDateFilterAndConfiguredTimeZone_ReturnsReservationsInLocalDay()
    {
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var complex = SportsComplex.Create(
            "Test Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            "test@example.com",
            "America/Montevideo");
        await _sportsComplexRepository.AddAsync(complex);

        var included = Reservation.Create(complex.Id, courtId, userId, new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc), ReservationSource.Web);
        var excluded = Reservation.Create(complex.Id, courtId, userId, new DateTime(2026, 8, 11, 14, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 11, 15, 0, 0, DateTimeKind.Utc), ReservationSource.Web);
        await _reservationRepository.AddAsync(included);
        await _reservationRepository.AddAsync(excluded);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetReservationsByComplexQuery(
            complex.Id,
            1,
            10,
            date: new DateOnly(2026, 8, 10)));

        Assert.Single(result.Items);
        Assert.Equal(included.Id, result.Items[0].Id);
    }
}
