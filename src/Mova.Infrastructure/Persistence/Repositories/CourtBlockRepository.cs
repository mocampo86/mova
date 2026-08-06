using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class CourtBlockRepository(MovaDbContext context) : ICourtBlockRepository
{
    public async Task<IReadOnlyCollection<CourtBlock>> GetForCourtAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await context.CourtBlocks
            .Where(b => b.CourtId == courtId)
            .Where(b => b.StartAt < end && b.EndAt > start)
            .ToArrayAsync(cancellationToken);
    }
}
