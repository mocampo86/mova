using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class ComplexAdministratorRepository : IComplexAdministratorRepository
{
    private readonly MovaDbContext _context;

    public ComplexAdministratorRepository(MovaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ComplexAdministrator>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ComplexAdministrators
            .AsNoTracking()
            .Where(ca => ca.UserId == userId && ca.Status == Domain.Enums.AdministratorStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public Task<ComplexAdministrator?> GetByUserAndComplexAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return _context.ComplexAdministrators
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ca => ca.UserId == userId && ca.SportsComplexId == sportsComplexId && ca.Status == Domain.Enums.AdministratorStatus.Active,
                cancellationToken);
    }

    public async Task AddAsync(ComplexAdministrator complexAdministrator, CancellationToken cancellationToken = default)
    {
        await _context.ComplexAdministrators.AddAsync(complexAdministrator, cancellationToken);
    }
}
