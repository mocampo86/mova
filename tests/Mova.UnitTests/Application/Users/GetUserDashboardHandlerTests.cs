using Mova.Application.Reservations.Handlers;
using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class GetUserDashboardHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();

    private GetUserDashboardHandler CreateHandler() =>
        new(_userRepository, _reservationRepository, _blockedUserRepository, _sportsComplexRepository);

    [Fact]
    public async Task HandleAsync_WithNoData_ReturnsDashboardWithUserInfo()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetUserDashboardQuery(user.Id, 1, 5, 3));

        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal(user.FullName, result.User.FullName);
        Assert.Empty(result.UpcomingReservations.Items);
        Assert.Equal(0, result.HistorySummary.TotalItems);
        Assert.Empty(result.ActiveBlocks);
    }

    [Fact]
    public async Task HandleAsync_WithUpcomingAndHistory_ReturnsBoth()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var now = DateTime.UtcNow;
        var upcomingReservation = Reservation.Create(complex.Id, Guid.NewGuid(), user.Id, now.AddHours(2), now.AddHours(3), ReservationSource.Web);
        var historyReservation = Reservation.Create(complex.Id, Guid.NewGuid(), user.Id, now.AddHours(-2), now.AddHours(-1), ReservationSource.Web);

        await _reservationRepository.AddAsync(upcomingReservation);
        await _reservationRepository.AddAsync(historyReservation);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetUserDashboardQuery(user.Id, 1, 5, 3));

        Assert.Single(result.UpcomingReservations.Items);
        Assert.Equal(upcomingReservation.Id, result.UpcomingReservations.Items[0].Id);
        Assert.Equal(1, result.HistorySummary.TotalItems);
        Assert.Single(result.HistorySummary.RecentReservations);
        Assert.Equal(historyReservation.Id, result.HistorySummary.RecentReservations[0].Id);
    }

    [Fact]
    public async Task HandleAsync_WithActiveBlock_ReturnsBlockWithComplexName()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var blockedByUserId = Guid.NewGuid();
        await _blockedUserRepository.AddAsync(BlockedUser.Create(complex.Id, user.Id, blockedByUserId, "No show"));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetUserDashboardQuery(user.Id, 1, 5, 3));

        Assert.Single(result.ActiveBlocks);
        Assert.Equal("Complex", result.ActiveBlocks[0].ComplexName);
        Assert.Equal("No show", result.ActiveBlocks[0].Reason);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<Mova.Application.Common.Exceptions.NotFoundException>(
            () => handler.HandleAsync(new GetUserDashboardQuery(Guid.NewGuid(), 1, 5, 3)));
    }
}
