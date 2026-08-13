using Mova.Application.Abstractions.Policies;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class CancelMyReservationHandlerTests
{
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CancelMyReservationHandler CreateHandler(int minimumHours = 0, bool allowUserCancellation = true) =>
        new(_reservationRepository, new FakeCancellationPolicy(minimumHours, allowUserCancellation), _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithinCancellationWindow_CancelsAndSetsUserStatus()
    {
        var userId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(2);
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            start,
            start.AddHours(1),
            ReservationSource.Web);
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler(24);
        var result = await handler.HandleAsync(new CancelMyReservationCommand(
            reservation.Id,
            userId,
            "Changed plans"));

        Assert.NotNull(result);
        Assert.Equal("CancelledByUser", result.Status);
        Assert.Equal("Changed plans", result.CancellationReason);
        Assert.Equal(userId, result.CancelledByUserId);
    }

    [Fact]
    public async Task HandleAsync_AfterCancellationWindow_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(1);
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            start,
            start.AddHours(1),
            ReservationSource.Web);
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler(24);

        await Assert.ThrowsAsync<CancellationDeadlineExceededException>(() => handler.HandleAsync(new CancelMyReservationCommand(
            reservation.Id,
            userId,
            "Too late")));
    }

    [Fact]
    public async Task HandleAsync_ReservationOwnedByAnotherUser_ThrowsNotFoundException()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            ReservationSource.Web);
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CancelMyReservationCommand(
            reservation.Id,
            Guid.NewGuid(),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithCompletedReservation_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(2);
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            start,
            start.AddHours(1),
            ReservationSource.Web);
        reservation.Confirm();
        reservation.MarkCompleted();
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler(24);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CancelMyReservationCommand(
            reservation.Id,
            userId,
            "Changed plans")));
    }

    [Fact]
    public async Task HandleAsync_NonExistentReservation_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CancelMyReservationCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null)));
    }

    [Fact]
    public async Task HandleAsync_UserCancellationDisabled_ThrowsCancellationDeadlineExceededException()
    {
        var userId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(2);
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            start,
            start.AddHours(1),
            ReservationSource.Web);
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler(24, false);

        await Assert.ThrowsAsync<CancellationDeadlineExceededException>(() => handler.HandleAsync(new CancelMyReservationCommand(
            reservation.Id,
            userId,
            "No user cancellations allowed")));
    }

    private sealed class FakeCancellationPolicy(int minimumHours, bool allowUserCancellation) : ICancellationPolicy
    {
        public bool IsWithinCancellationWindow(DateTime startAt, DateTime now)
        {
            if (!allowUserCancellation)
            {
                return false;
            }

            return now <= startAt.AddHours(-minimumHours);
        }
    }
}
