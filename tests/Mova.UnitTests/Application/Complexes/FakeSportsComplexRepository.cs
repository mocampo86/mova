using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeSportsComplexRepository : ISportsComplexRepository
{
    private readonly List<SportsComplex> _sportsComplexes = [];

    public Task<SportsComplex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sportsComplexes.FirstOrDefault(s => s.Id == id));
    }

    public Task AddAsync(SportsComplex sportsComplex, CancellationToken cancellationToken = default)
    {
        _sportsComplexes.Add(sportsComplex);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<SportsComplex> Items, int TotalItems)> GetActiveComplexesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _sportsComplexes
            .Where(s => s.Status == ComplexStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .ToList();

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;

        var totalItems = query.Count;
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult((items as IReadOnlyList<SportsComplex>, totalItems));
    }
}
