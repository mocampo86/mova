using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Exceptions;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class CreateReservationHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeCourtRepository _courtRepository = new();
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeCourtBlockRepository _courtBlockRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateReservationHandler CreateHandler() =>
        new(
            _sportsComplexRepository,
            _courtRepository,
            _userRepository,
            _blockedUserRepository,
            _reservationRepository,
            _courtBlockRepository,
            _auditLogs,
            _unitOfWork);

    private async Task<(SportsComplex Complex, Court Court, User User)> CreateScenarioAsync()
    {
        var complex = SportsComplex.Create(
            "Test Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            $"test-{Guid.NewGuid()}@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var court = Court.Create(complex.Id, "Court 1", "Description", "Synthetic", false);
        await _courtRepository.AddAsync(court);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        return (complex, court, user);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesReservationAndAuditLog()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var start = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);
        var createdByUserId = Guid.NewGuid();
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new CreateReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            createdByUserId,
            start,
            end,
            "Notes"));

        Assert.NotNull(result);
        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(court.Id, result.CourtId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Confirmed", result.Status);
        Assert.Equal("Admin", result.Source);
        Assert.NotNull(result.UpdatedAt);

        var auditLog = Assert.Single(_auditLogs.AuditLogs);
        Assert.Equal("Reservation.Create", auditLog.Action);
        Assert.Equal("Reservation", auditLog.EntityType);
        Assert.Equal(result.Id.ToString(), auditLog.EntityId);
        Assert.Equal(complex.Id, auditLog.SportsComplexId);
        Assert.Equal(createdByUserId, auditLog.UserId);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var (_, court, user) = await CreateScenarioAsync();
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CreateReservationCommand(
            Guid.NewGuid(),
            court.Id,
            user.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithInactiveComplex_ThrowsConflictException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        complex.Deactivate();
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CreateReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithCourtFromDifferentComplex_ThrowsNotFoundException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var otherComplex = SportsComplex.Create(
            "Other",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            $"other-{Guid.NewGuid()}@example.com");
        await _sportsComplexRepository.AddAsync(otherComplex);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CreateReservationCommand(
            otherComplex.Id,
            court.Id,
            user.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCourt_ThrowsConflictException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        court.Deactivate();
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CreateReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithBlockedUser_ThrowsUserBlockedException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var blockedByUserId = Guid.NewGuid();
        await _blockedUserRepository.AddAsync(BlockedUser.Create(
            complex.Id,
            user.Id,
            blockedByUserId,
            "Spam"));
        var handler = CreateHandler();

        await Assert.ThrowsAsync<UserBlockedException>(() => handler.HandleAsync(new CreateReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            blockedByUserId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithExpiredBlock_CreatesReservation()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var blockedByUserId = Guid.NewGuid();
        await _blockedUserRepository.AddAsync(BlockedUser.Create(
            complex.Id,
            user.Id,
            blockedByUserId,
            "Spam",
            DateTime.UtcNow.AddDays(-1)));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new CreateReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            blockedByUserId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            null));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task HandleAsync_WithOverlappingReservation_ThrowsConflictException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var start = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);
        var existing = Reservation.Create(complex.Id, court.Id, user.Id, start, end, ReservationSource.Web);
        await _reservationRepository.AddAsync(existing);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CreateReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            Guid.NewGuid(),
            start.AddMinutes(30),
            end.AddMinutes(30),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithOverlappingCourtBlock_ThrowsConflictException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var start = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);
        var block = CourtBlock.Create(complex.Id, court.Id, start, end, Guid.NewGuid(), "Maintenance");
        _courtBlockRepository.Add(block);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CreateReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            Guid.NewGuid(),
            start,
            end,
            null)));
    }
}
