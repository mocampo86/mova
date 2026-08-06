using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class CourtAvailabilityRuleRepository(MovaDbContext context) : ICourtAvailabilityRuleRepository
{
    public async Task<IReadOnlyCollection<CourtAvailabilityRule>> GetByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default) =>
        await context.CourtAvailabilityRules.Where(x => x.CourtId == courtId).ToArrayAsync(cancellationToken);

    public Task AddAsync(CourtAvailabilityRule rule, CancellationToken cancellationToken = default) =>
        context.CourtAvailabilityRules.AddAsync(rule, cancellationToken).AsTask();

    public void RemoveRange(IEnumerable<CourtAvailabilityRule> rules) =>
        context.CourtAvailabilityRules.RemoveRange(rules);
}
