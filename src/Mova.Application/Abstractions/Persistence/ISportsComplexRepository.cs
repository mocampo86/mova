using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ISportsComplexRepository
{
    Task<SportsComplex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(SportsComplex sportsComplex, CancellationToken cancellationToken = default);
}
