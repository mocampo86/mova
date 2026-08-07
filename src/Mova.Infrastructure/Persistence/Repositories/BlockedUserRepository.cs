using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class BlockedUserRepository(MovaDbContext context) : IBlockedUserRepository
{
    public Task<int> CountActiveByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return context.BlockedUsers
            .CountAsync(b => b.SportsComplexId == sportsComplexId && b.Status == BlockedUserStatus.Active, cancellationToken);
    }
}
