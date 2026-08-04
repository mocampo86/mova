using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class SportsComplexRepository : ISportsComplexRepository
{
    private readonly MovaDbContext _context;

    public SportsComplexRepository(MovaDbContext context)
    {
        _context = context;
    }

    public Task<SportsComplex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.SportsComplexes
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(SportsComplex sportsComplex, CancellationToken cancellationToken = default)
    {
        await _context.SportsComplexes.AddAsync(sportsComplex, cancellationToken);
    }
}
