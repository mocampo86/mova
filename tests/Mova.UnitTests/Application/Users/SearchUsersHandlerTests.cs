using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class SearchUsersHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();

    private SearchUsersHandler CreateHandler() =>
        new(_sportsComplexRepository, _userRepository, _blockedUserRepository);

    [Fact]
    public async Task HandleAsync_WithMatchingSearch_ReturnsPagedUsersWithBlockStatus()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        user.CompleteProfile("+598 99 999 999");
        await _userRepository.AddAsync(user);

        var blockedByUserId = Guid.NewGuid();
        await _blockedUserRepository.AddAsync(BlockedUser.Create(complex.Id, user.Id, blockedByUserId, "Spam"));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new SearchUsersQuery(complex.Id, "Test", 1, 10));

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(user.Id, result.Items[0].Id);
        Assert.True(result.Items[0].IsBlocked);
        Assert.Equal("Spam", result.Items[0].BlockReason);
    }

    [Fact]
    public async Task HandleAsync_WithPhoneNumberSearch_ReturnsMatchingUser()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Searchable User");
        user.CompleteProfile("+54 11 1234 5678");
        await _userRepository.AddAsync(user);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new SearchUsersQuery(complex.Id, "+54 11 1234", 1, 10));

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(user.Id, result.Items[0].Id);
        Assert.Equal("+54 11 1234 5678", result.Items[0].PhoneNumber);
    }

    [Fact]
    public async Task HandleAsync_WithBlockedGlobalUser_DoesNotReturnUser()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Blocked User");
        user.Block();
        await _userRepository.AddAsync(user);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new SearchUsersQuery(complex.Id, "Blocked", 1, 10));

        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task HandleAsync_WithNormalizedPhoneDigits_ReturnsMatchingUser()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Searchable User");
        user.CompleteProfile("+54 11 1234 5678");
        await _userRepository.AddAsync(user);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new SearchUsersQuery(complex.Id, "541112345678", 1, 10));

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(user.Id, result.Items[0].Id);
        Assert.Equal("+54 11 1234 5678", result.Items[0].PhoneNumber);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<Mova.Application.Common.Exceptions.NotFoundException>(
            () => handler.HandleAsync(new SearchUsersQuery(Guid.NewGuid(), null, 1, 10)));
    }
}
