using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ICourtAvailabilityRuleRepository
{
    Task<IReadOnlyCollection<CourtAvailabilityRule>> GetByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default);
    Task AddAsync(CourtAvailabilityRule rule, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<CourtAvailabilityRule> rules);
}
