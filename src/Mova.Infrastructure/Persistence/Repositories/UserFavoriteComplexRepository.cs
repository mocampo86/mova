using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class UserFavoriteComplexRepository : IUserFavoriteComplexRepository
{
    private const int MaxPageSize = 100;

    private readonly MovaDbContext _context;

    public UserFavoriteComplexRepository(MovaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Guid>> GetComplexIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFavoriteComplexes
            .Where(x => x.UserId == userId)
            .Select(x => x.SportsComplexId)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<FavoriteComplexItem> Items, int TotalItems)> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        var query = _context.UserFavoriteComplexes
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Join(_context.SportsComplexes, x => x.SportsComplexId, s => s.Id, (x, s) => new { Favorite = x, SportsComplex = s });

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var complexIds = items.Select(i => i.SportsComplex.Id).ToList();
        var sportRows = await _context.Courts
            .Where(c => complexIds.Contains(c.SportsComplexId) && c.Status == CourtStatus.Active)
            .SelectMany(c => c.CourtSports.Select(cs => new { c.SportsComplexId, cs.Sport.Name }))
            .ToListAsync(cancellationToken);
        var sportsByComplex = sportRows.Distinct().ToLookup(x => x.SportsComplexId, x => x.Name);

        var result = items
            .Select(i => new FavoriteComplexItem(i.Favorite, i.SportsComplex, sportsByComplex[i.SportsComplex.Id].ToList()))
            .ToList();

        return (result, totalItems);
    }

    public Task<bool> ExistsAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return _context.UserFavoriteComplexes
            .AnyAsync(x => x.UserId == userId && x.SportsComplexId == sportsComplexId, cancellationToken);
    }

    public async Task<bool> AddIfNotExistsAsync(UserFavoriteComplex favorite, CancellationToken cancellationToken = default)
    {
        await _context.UserFavoriteComplexes.AddAsync(favorite, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            _context.Entry(favorite).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<bool> RemoveIfExistsAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        var deleted = await _context.UserFavoriteComplexes
            .Where(x => x.UserId == userId && x.SportsComplexId == sportsComplexId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }
}
