using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ICancellationPolicyRepository
{
    Task<CancellationPolicy?> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default);

    Task AddAsync(CancellationPolicy cancellationPolicy, CancellationToken cancellationToken = default);
}
