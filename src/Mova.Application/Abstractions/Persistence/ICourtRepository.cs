using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Abstractions.Persistence;

public interface ICourtRepository
{
    Task AddAsync(Court court, CancellationToken cancellationToken = default);
    Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Court?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Court> Items, int TotalItems)> GetActiveCourtsByComplexIdAsync(Guid sportsComplexId, int page, int pageSize, Guid? sportId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Court> Items, int TotalItems)> GetCourtsByComplexIdAsync(Guid sportsComplexId, int page, int pageSize, Guid? sportId = null, CourtStatus? status = null, CancellationToken cancellationToken = default);
    Task<(int ActiveCount, int InactiveCount)> GetCourtStatusCountsByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default);
}
