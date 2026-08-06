using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class SportsComplexRepository : ISportsComplexRepository
{
    private const int MaxPageSize = 100;

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

    public async Task<(IReadOnlyList<SportsComplex> Items, int TotalItems)> GetActiveComplexesAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        IQueryable<SportsComplex> query = _context.SportsComplexes
            .Where(s => s.Status == ComplexStatus.Active)
            .OrderByDescending(s => s.CreatedAt);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var pattern = $"%{normalizedSearch}%";
            query = query.Where(s => EF.Functions.ILike(s.Name, pattern)
                || EF.Functions.ILike(s.City, pattern)
                || EF.Functions.ILike(s.Address, pattern));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<(IReadOnlyList<SportsComplex> Items, int TotalItems)> GetAllComplexesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        var query = _context.SportsComplexes
            .OrderByDescending(s => s.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }
}
