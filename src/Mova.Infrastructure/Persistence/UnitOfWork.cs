using Mova.Application.Abstractions.Persistence;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MovaDbContext _context;

    public UnitOfWork(MovaDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
