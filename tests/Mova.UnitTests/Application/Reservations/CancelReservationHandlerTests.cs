using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class CancelReservationHandlerTests
{
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CancelReservationHandler CreateHandler() => new(_reservationRepository, _auditLogs, _unitOfWork);

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

        var actorId = Guid.NewGuid();
        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CancelReservationCommand(
            reservation.SportsComplexId,
            reservation.Id,
            actorId,
            "No show"));

        Assert.NotNull(result);
        Assert.Equal("CancelledByAdmin", result.Status);
        Assert.Equal("No show", result.CancellationReason);
        Assert.Equal(actorId, result.CancelledByUserId);
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

    [Fact]
    public async Task HandleAsync_WithCompletedReservation_ThrowsConflictException()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        reservation.Confirm();
        reservation.MarkCompleted();
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CancelReservationCommand(
            reservation.SportsComplexId,
            reservation.Id,
            Guid.NewGuid(),
            "No show")));
    }

    [Fact]
    public async Task HandleAsync_WithCancelledByAdminReservation_IsIdempotent()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);

        var actorId = Guid.NewGuid();
        reservation.Cancel(actorId, true, "No show");
        await _reservationRepository.AddAsync(reservation);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CancelReservationCommand(
            reservation.SportsComplexId,
            reservation.Id,
            actorId,
            "No show"));

        Assert.NotNull(result);
        Assert.Equal("CancelledByAdmin", result.Status);
        Assert.Equal("No show", result.CancellationReason);
        Assert.Equal(actorId, result.CancelledByUserId);
    }

    [Fact]
    public async Task HandleAsync_RecurringOccurrence_CancelsOnlyThatOccurrenceAndTracksActor()
    {
        var complexId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recurringReservationId = Guid.NewGuid();

        var first = Reservation.Create(
            complexId,
            courtId,
            userId,
            new DateTime(2030, 1, 7, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 7, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Recurring,
            null,
            recurringReservationId);
        first.Confirm();

        var second = Reservation.Create(
            complexId,
            courtId,
            userId,
            new DateTime(2030, 1, 14, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 14, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Recurring,
            null,
            recurringReservationId);
        second.Confirm();

        await _reservationRepository.AddAsync(first);
        await _reservationRepository.AddAsync(second);

        var actorId = Guid.NewGuid();
        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CancelReservationCommand(
            complexId,
            first.Id,
            actorId,
            "No show"));

        Assert.NotNull(result);
        Assert.Equal("CancelledByAdmin", result.Status);
        Assert.Equal(recurringReservationId, result.RecurringReservationId);
        Assert.Equal("No show", result.CancellationReason);
        Assert.Equal(actorId, result.CancelledByUserId);

        var other = await _reservationRepository.GetByIdAsync(second.Id);
        Assert.NotNull(other);
        Assert.Equal("Confirmed", other.Status.ToString());
    }
}
