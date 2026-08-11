using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeCourtAvailabilityRuleRepository : ICourtAvailabilityRuleRepository
{
    private readonly List<CourtAvailabilityRule> _rules = [];

    public Task AddAsync(CourtAvailabilityRule rule, CancellationToken cancellationToken = default)
    {
        _rules.Add(rule);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CourtAvailabilityRule>> GetByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<CourtAvailabilityRule>>(_rules.Where(x => x.CourtId == courtId).ToList());
    }

    public void RemoveRange(IEnumerable<CourtAvailabilityRule> rules)
    {
        foreach (var item in rules)
        {
            _rules.Remove(item);
        }
    }
}
