using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class GetUsersByComplexHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();

    private GetUsersByComplexHandler CreateHandler() =>
        new(_sportsComplexRepository, _userRepository, _blockedUserRepository);

    [Fact]
    public async Task HandleAsync_WithUsers_ReturnsPagedUsersWithBlockStatus()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var blockedByUserId = Guid.NewGuid();
        await _blockedUserRepository.AddAsync(BlockedUser.Create(complex.Id, user.Id, blockedByUserId, "Spam"));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetUsersByComplexQuery(complex.Id, 1, 10));

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(user.Id, result.Items[0].Id);
        Assert.True(result.Items[0].IsBlocked);
        Assert.Equal("Spam", result.Items[0].BlockReason);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<Mova.Application.Common.Exceptions.NotFoundException>(
            () => handler.HandleAsync(new GetUsersByComplexQuery(Guid.NewGuid(), 1, 10)));
    }
}
