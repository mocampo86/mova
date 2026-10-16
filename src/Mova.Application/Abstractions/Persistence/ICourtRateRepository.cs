using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ICourtRateRepository
{
    Task<IReadOnlyCollection<CourtRate>> GetByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CourtRate>> GetActiveByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default);

    Task<CourtRate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(CourtRate rate, CancellationToken cancellationToken = default);
}
