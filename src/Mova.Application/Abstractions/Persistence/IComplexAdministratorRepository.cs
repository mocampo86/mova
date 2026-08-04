using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IComplexAdministratorRepository
{
    Task<IReadOnlyCollection<ComplexAdministrator>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ComplexAdministrator?> GetByUserAndComplexAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default);
    Task AddAsync(ComplexAdministrator complexAdministrator, CancellationToken cancellationToken = default);
}
