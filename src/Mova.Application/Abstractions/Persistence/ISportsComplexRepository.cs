using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ISportsComplexRepository
{
    Task<SportsComplex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(SportsComplex sportsComplex, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<SportsComplex> Items, int TotalItems)> GetActiveComplexesAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<SportsComplex> Items, int TotalItems)> GetAllComplexesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
