using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeBlockedUserRepository : IBlockedUserRepository
{
    private readonly List<BlockedUser> _blockedUsers = [];

    private static bool IsActive(BlockedUser b) =>
        b.Status == BlockedUserStatus.Active && (!b.BlockedUntil.HasValue || b.BlockedUntil.Value > DateTime.UtcNow);

    public Task AddAsync(BlockedUser blockedUser, CancellationToken cancellationToken = default)
    {
        _blockedUsers.Add(blockedUser);
        return Task.CompletedTask;
    }

    public Task<BlockedUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_blockedUsers.FirstOrDefault(b => b.Id == id));
    }

    public Task<BlockedUser?> GetActiveByComplexAndUserAsync(Guid sportsComplexId, Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_blockedUsers.FirstOrDefault(b =>
            b.SportsComplexId == sportsComplexId && b.UserId == userId && IsActive(b)));
    }

    public Task<IReadOnlyList<BlockedUser>> GetActiveBlocksByComplexAndUserIdsAsync(
        Guid sportsComplexId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var blocks = _blockedUsers
            .Where(b => b.SportsComplexId == sportsComplexId && userIds.Contains(b.UserId) && IsActive(b))
            .ToList();

        return Task.FromResult<IReadOnlyList<BlockedUser>>(blocks);
    }

    public Task<IReadOnlyList<BlockedUser>> GetActiveBlocksByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var blocks = _blockedUsers
            .Where(b => b.UserId == userId && IsActive(b))
            .ToList();

        return Task.FromResult<IReadOnlyList<BlockedUser>>(blocks);
    }

    public Task<int> CountActiveByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        var count = _blockedUsers.Count(b => b.SportsComplexId == sportsComplexId && IsActive(b));
        return Task.FromResult(count);
    }

    public Task<bool> IsUserBlockedAsync(Guid sportsComplexId, Guid userId, CancellationToken cancellationToken = default)
    {
        var blocked = _blockedUsers.Any(b => b.SportsComplexId == sportsComplexId && b.UserId == userId && IsActive(b));
        return Task.FromResult(blocked);
    }
}
