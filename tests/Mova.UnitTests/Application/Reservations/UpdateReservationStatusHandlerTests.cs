using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class UpdateReservationStatusHandlerTests
{
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateReservationStatusHandler CreateHandler() => new(_reservationRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithCompletedStatus_MarksReservationCompleted()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateReservationStatusCommand(
            reservation.SportsComplexId,
            reservation.Id,
            ReservationStatus.Completed));

        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithNoShowStatus_MarksReservationNoShow()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateReservationStatusCommand(
            reservation.SportsComplexId,
            reservation.Id,
            ReservationStatus.NoShow));

        Assert.NotNull(result);
        Assert.Equal("NoShow", result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithCancelledReservation_ThrowsConflictException()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        reservation.Cancel(Guid.NewGuid(), true, "Reason");
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new UpdateReservationStatusCommand(
            reservation.SportsComplexId,
            reservation.Id,
            ReservationStatus.Completed)));
    }

    [Fact]
    public async Task HandleAsync_WithReservationFromDifferentComplex_ThrowsNotFoundException()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateReservationStatusCommand(
            Guid.NewGuid(),
            reservation.Id,
            ReservationStatus.Completed)));
    }
}
