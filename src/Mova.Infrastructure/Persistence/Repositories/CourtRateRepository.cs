using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class CourtRateRepository(MovaDbContext context) : ICourtRateRepository
{
    public async Task<IReadOnlyCollection<CourtRate>> GetByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default) =>
        await context.CourtRates.Where(x => x.CourtId == courtId).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<CourtRate>> GetActiveByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default) =>
        await context.CourtRates.Where(x => x.CourtId == courtId && x.IsActive).ToArrayAsync(cancellationToken);

    public Task<CourtRate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.CourtRates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(CourtRate rate, CancellationToken cancellationToken = default) =>
        context.CourtRates.AddAsync(rate, cancellationToken).AsTask();
}
