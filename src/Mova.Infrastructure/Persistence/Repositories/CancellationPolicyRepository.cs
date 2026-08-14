using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class CancellationPolicyRepository(MovaDbContext context) : ICancellationPolicyRepository
{
    public Task<CancellationPolicy?> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return context.CancellationPolicies
            .FirstOrDefaultAsync(x => x.SportsComplexId == sportsComplexId, cancellationToken);
    }

    public Task AddAsync(CancellationPolicy cancellationPolicy, CancellationToken cancellationToken = default)
    {
        return context.CancellationPolicies.AddAsync(cancellationPolicy, cancellationToken).AsTask();
    }
}
