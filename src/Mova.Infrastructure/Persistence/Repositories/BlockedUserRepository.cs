using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class BlockedUserRepository(MovaDbContext context) : IBlockedUserRepository
{
    public Task AddAsync(BlockedUser blockedUser, CancellationToken cancellationToken = default)
    {
        return context.BlockedUsers.AddAsync(blockedUser, cancellationToken).AsTask();
    }

    public Task<BlockedUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.BlockedUsers
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public Task<BlockedUser?> GetActiveByComplexAndUserAsync(Guid sportsComplexId, Guid userId, CancellationToken cancellationToken = default)
    {
        return context.BlockedUsers
            .FirstOrDefaultAsync(
                b => b.SportsComplexId == sportsComplexId
                    && b.UserId == userId
                    && b.Status == BlockedUserStatus.Active
                    && (!b.BlockedUntil.HasValue || b.BlockedUntil.Value > DateTime.UtcNow),
                cancellationToken);
    }

    public Task<BlockedUser?> GetExpiredActiveByComplexAndUserAsync(Guid sportsComplexId, Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return context.BlockedUsers
            .FirstOrDefaultAsync(
                b => b.SportsComplexId == sportsComplexId
                    && b.UserId == userId
                    && b.Status == BlockedUserStatus.Active
                    && b.BlockedUntil.HasValue
                    && b.BlockedUntil.Value <= now,
                cancellationToken);
    }

    public async Task<IReadOnlyList<BlockedUser>> GetActiveBlocksByComplexAndUserIdsAsync(
        Guid sportsComplexId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await context.BlockedUsers
            .Where(b => b.SportsComplexId == sportsComplexId
                && userIds.Contains(b.UserId)
                && b.Status == BlockedUserStatus.Active
                && (!b.BlockedUntil.HasValue || b.BlockedUntil.Value > now))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BlockedUser>> GetActiveBlocksByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await context.BlockedUsers
            .Where(b => b.UserId == userId
                && b.Status == BlockedUserStatus.Active
                && (!b.BlockedUntil.HasValue || b.BlockedUntil.Value > now))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountActiveByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return context.BlockedUsers
            .CountAsync(b => b.SportsComplexId == sportsComplexId
                && b.Status == BlockedUserStatus.Active
                && (!b.BlockedUntil.HasValue || b.BlockedUntil.Value > now),
                cancellationToken);
    }

    public async Task<bool> IsUserBlockedAsync(Guid sportsComplexId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetActiveByComplexAndUserAsync(sportsComplexId, userId, cancellationToken) is not null;
    }
}
