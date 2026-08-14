using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class CreateRecurringReservationHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeCourtRepository _courtRepository = new();
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();
    private readonly FakeRecurringReservationRepository _recurringReservationRepository = new();
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeCourtBlockRepository _courtBlockRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateRecurringReservationHandler CreateHandler() =>
        new(
            _sportsComplexRepository,
            _courtRepository,
            _userRepository,
            _blockedUserRepository,
            _recurringReservationRepository,
            _reservationRepository,
            _courtBlockRepository,
            _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithValidWeeklyPeriod_CreatesConfirmedOccurrences()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new CreateRecurringReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 31),
            "Weekly slot"));

        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(court.Id, result.CourtId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Active", result.Status);
        Assert.Equal(4, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence =>
        {
            Assert.Equal("Confirmed", occurrence.Status);
            Assert.Equal("Recurring", occurrence.Source);
            Assert.Equal(result.Id, occurrence.RecurringReservationId);
        });
    }

    [Fact]
    public async Task HandleAsync_WithOverlappingOccurrence_ThrowsConflictException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        var existing = Reservation.Create(
            complex.Id,
            court.Id,
            user.Id,
            new DateTime(2026, 8, 17, 14, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 17, 15, 30, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        existing.Confirm();
        await _reservationRepository.AddAsync(existing);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CreateRecurringReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 31),
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithBlockedOccurrence_ThrowsConflictException()
    {
        var (complex, court, user) = await CreateScenarioAsync();
        _courtBlockRepository.Add(CourtBlock.Create(
            complex.Id,
            court.Id,
            new DateTime(2026, 8, 24, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 24, 15, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid(),
            "Maintenance"));
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new CreateRecurringReservationCommand(
            complex.Id,
            court.Id,
            user.Id,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 31),
            null)));
    }

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
}
