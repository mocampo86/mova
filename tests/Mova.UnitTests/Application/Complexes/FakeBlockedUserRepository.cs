using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeBlockedUserRepository : IBlockedUserRepository
{
    private readonly List<BlockedUser> _blockedUsers = [];

    public Task AddAsync(BlockedUser blockedUser, CancellationToken cancellationToken = default)
    {
        _blockedUsers.Add(blockedUser);
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        var count = _blockedUsers.Count(b => b.SportsComplexId == sportsComplexId && b.Status == BlockedUserStatus.Active);
        return Task.FromResult(count);
    }
}
