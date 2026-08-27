using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class GetMyBlockStatusHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();

    private GetMyBlockStatusHandler CreateHandler() =>
        new(_sportsComplexRepository, _blockedUserRepository);

    [Fact]
    public async Task HandleAsync_WhenNotBlocked_ReturnsIsBlockedFalse()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetMyBlockStatusQuery(user.Id, complex.Id));

        Assert.False(result.IsBlocked);
        Assert.Equal(complex.Id, result.ComplexId);
        Assert.Equal(complex.Name, result.ComplexName);
        Assert.Null(result.Reason);
    }

    [Fact]
    public async Task HandleAsync_WhenBlocked_ReturnsBlockDetails()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        var blockedByUserId = Guid.NewGuid();
        await _blockedUserRepository.AddAsync(BlockedUser.Create(complex.Id, user.Id, blockedByUserId, "No show"));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetMyBlockStatusQuery(user.Id, complex.Id));

        Assert.True(result.IsBlocked);
        Assert.Equal(complex.Id, result.ComplexId);
        Assert.Equal(complex.Name, result.ComplexName);
        Assert.Equal("No show", result.Reason);
        Assert.NotEqual(default, result.BlockedAt);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(new GetMyBlockStatusQuery(Guid.NewGuid(), Guid.NewGuid())));
    }
}
