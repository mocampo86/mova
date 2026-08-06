using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class CourtRepository(MovaDbContext context) : ICourtRepository
{
    public Task AddAsync(Court court, CancellationToken cancellationToken = default) => context.Courts.AddAsync(court, cancellationToken).AsTask();

    public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) =>
        context.Courts.AnyAsync(x => x.SportsComplexId == sportsComplexId && x.Name.ToLower() == name.Trim().ToLower(), cancellationToken);
}
