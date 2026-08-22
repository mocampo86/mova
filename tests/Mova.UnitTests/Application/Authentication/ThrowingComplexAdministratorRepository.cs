using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Authentication;

public sealed class ThrowingComplexAdministratorRepository : IComplexAdministratorRepository
{
    public Task<IReadOnlyCollection<ComplexAdministrator>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<ComplexAdministrator>>([]);
    }

    public Task<ComplexAdministrator?> GetByUserAndComplexAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ComplexAdministrator?>(null);
    }

    public Task AddAsync(ComplexAdministrator complexAdministrator, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated administrator failure.");
    }
}
