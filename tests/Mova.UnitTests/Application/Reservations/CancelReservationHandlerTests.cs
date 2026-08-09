using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class CancelReservationHandlerTests
{
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CancelReservationHandler CreateHandler() => new(_reservationRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithValidReservation_CancelsAndSetsAdminStatus()
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
        var result = await handler.HandleAsync(new CancelReservationCommand(
            reservation.SportsComplexId,
            reservation.Id,
            Guid.NewGuid(),
            "No show"));

        Assert.NotNull(result);
        Assert.Equal("CancelledByAdmin", result.Status);
        Assert.Equal("No show", result.CancellationReason);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentReservation_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CancelReservationCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null)));
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

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CancelReservationCommand(
            Guid.NewGuid(),
            reservation.Id,
            Guid.NewGuid(),
            null)));
    }
}
