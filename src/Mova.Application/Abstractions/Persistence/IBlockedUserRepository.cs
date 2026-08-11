using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IBlockedUserRepository
{
    Task AddAsync(BlockedUser blockedUser, CancellationToken cancellationToken = default);
    Task<BlockedUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BlockedUser?> GetActiveByComplexAndUserAsync(Guid sportsComplexId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BlockedUser>> GetActiveBlocksByComplexAndUserIdsAsync(Guid sportsComplexId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BlockedUser>> GetActiveBlocksByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountActiveByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default);
    Task<bool> IsUserBlockedAsync(Guid sportsComplexId, Guid userId, CancellationToken cancellationToken = default);
}
