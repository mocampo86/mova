using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class UnblockUserHandlerTests
{
    private readonly FakeBlockedUserRepository _blockedUserRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeCurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UnblockUserHandler CreateHandler() => new(_blockedUserRepository, _auditLogs, _currentUser, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithExistingBlock_LiftsBlock()
    {
        var complexId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var blockedByUserId = Guid.NewGuid();

        var block = BlockedUser.Create(complexId, userId, blockedByUserId, "Spam");
        await _blockedUserRepository.AddAsync(block);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UnblockUserCommand(complexId, block.Id));

        Assert.Equal("Lifted", result.Status);
        Assert.False(await _blockedUserRepository.IsUserBlockedAsync(complexId, userId));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentBlock_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(new UnblockUserCommand(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WithBlockFromDifferentComplex_ThrowsNotFoundException()
    {
        var block = BlockedUser.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Spam");
        await _blockedUserRepository.AddAsync(block);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(new UnblockUserCommand(Guid.NewGuid(), block.Id)));
    }
}
