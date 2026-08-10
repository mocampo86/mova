using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class CourtRepository(MovaDbContext context) : ICourtRepository
{
    private const int MaxPageSize = 100;

    public Task AddAsync(Court court, CancellationToken cancellationToken = default) => context.Courts.AddAsync(court, cancellationToken).AsTask();

    public Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Courts.Include(x => x.CourtSports).ThenInclude(x => x.Sport).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Court?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Courts
            .Include(x => x.CourtSports)
            .ThenInclude(x => x.Sport)
            .SingleOrDefaultAsync(x => x.Id == id && x.Status == CourtStatus.Active && x.SportsComplex!.Status == ComplexStatus.Active, cancellationToken);

    public async Task<(IReadOnlyList<Court> Items, int TotalItems)> GetActiveCourtsByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? sportId = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        IQueryable<Court> query = context.Courts
            .Include(x => x.CourtSports)
            .ThenInclude(x => x.Sport)
            .Where(x => x.SportsComplexId == sportsComplexId && x.Status == CourtStatus.Active && x.SportsComplex!.Status == ComplexStatus.Active)
            .OrderByDescending(x => x.CreatedAt);

        if (sportId.HasValue)
        {
            query = query.Where(x => x.CourtSports.Any(cs => cs.SportId == sportId.Value && cs.Sport.Status == SportStatus.Active));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) =>
        context.Courts.AnyAsync(x => x.SportsComplexId == sportsComplexId && x.Name.ToLower() == name.Trim().ToLower(), cancellationToken);

    public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, Guid excludeCourtId, CancellationToken cancellationToken = default) =>
        context.Courts.AnyAsync(x => x.SportsComplexId == sportsComplexId && x.Id != excludeCourtId && x.Name.ToLower() == name.Trim().ToLower(), cancellationToken);

    public async Task<(IReadOnlyList<Court> Items, int TotalItems)> GetCourtsByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? sportId = null,
        CourtStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        IQueryable<Court> query = context.Courts
            .Include(x => x.CourtSports)
            .ThenInclude(x => x.Sport)
            .Where(x => x.SportsComplexId == sportsComplexId)
            .OrderByDescending(x => x.CreatedAt);

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (sportId.HasValue)
        {
            query = query.Where(x => x.CourtSports.Any(cs => cs.SportId == sportId.Value && cs.Sport.Status == SportStatus.Active));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<(int ActiveCount, int InactiveCount)> GetCourtStatusCountsByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        var counts = await context.Courts
            .Where(x => x.SportsComplexId == sportsComplexId)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var activeCount = counts.FirstOrDefault(c => c.Status == CourtStatus.Active)?.Count ?? 0;
        var inactiveCount = counts.FirstOrDefault(c => c.Status == CourtStatus.Inactive)?.Count ?? 0;

        return (activeCount, inactiveCount);
    }
}
