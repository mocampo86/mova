using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class SportRepository(MovaDbContext context) : ISportRepository
{
    public async Task<IReadOnlyCollection<Sport>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var requestedIds = ids.Distinct().ToArray();
        return await context.Sports.Where(x => requestedIds.Contains(x.Id)).ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Sport>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Sports
            .Where(x => x.Status == SportStatus.Active)
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }
}
