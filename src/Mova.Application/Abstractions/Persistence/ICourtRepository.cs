using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ICourtRepository
{
    Task AddAsync(Court court, CancellationToken cancellationToken = default);
    Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default);
}
