using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeCancellationPolicyRepository : ICancellationPolicyRepository
{
    private readonly List<CancellationPolicy> _policies = [];

    public Task<CancellationPolicy?> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_policies.FirstOrDefault(p => p.SportsComplexId == sportsComplexId));
    }

    public Task AddAsync(CancellationPolicy cancellationPolicy, CancellationToken cancellationToken = default)
    {
        _policies.Add(cancellationPolicy);
        return Task.CompletedTask;
    }
}
