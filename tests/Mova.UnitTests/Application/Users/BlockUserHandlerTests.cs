using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class BlockUserHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private BlockUserHandler CreateHandler() =>
        new(_sportsComplexRepository, _userRepository, _blockedUserRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesBlock()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var adminId = Guid.NewGuid();
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new BlockUserCommand(complex.Id, user.Id, adminId, "Spam"));

        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Spam", result.Reason);
        Assert.Equal("Active", result.Status);
        Assert.True(await _blockedUserRepository.IsUserBlockedAsync(complex.Id, user.Id));
    }

    [Fact]
    public async Task HandleAsync_WithExistingActiveBlock_ThrowsConflictException()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var adminId = Guid.NewGuid();
        await _blockedUserRepository.AddAsync(BlockedUser.Create(complex.Id, user.Id, adminId, "Spam"));

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(new BlockUserCommand(complex.Id, user.Id, adminId, "Other")));
    }

    [Fact]
    public async Task HandleAsync_WithExpiredBlock_LiftsItAndCreatesNewBlock()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var adminId = Guid.NewGuid();
        var expiredBlock = BlockedUser.Create(complex.Id, user.Id, adminId, "Expired", DateTime.UtcNow.AddDays(-1));
        await _blockedUserRepository.AddAsync(expiredBlock);

        var result = await CreateHandler().HandleAsync(new BlockUserCommand(complex.Id, user.Id, adminId, "New block"));

        Assert.Equal("Lifted", expiredBlock.Status.ToString());
        Assert.Equal("Active", result.Status);
        Assert.Equal(2, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@example.com", "Test User");
        await _userRepository.AddAsync(user);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(new BlockUserCommand(Guid.NewGuid(), user.Id, Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ThrowsNotFoundException()
    {
        var complex = SportsComplex.Create("Complex", "Description", "Address", "Montevideo", null, null, "+598 99 123 456", "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(new BlockUserCommand(complex.Id, Guid.NewGuid(), Guid.NewGuid())));
    }
}
