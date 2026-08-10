using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeCourtRepository : ICourtRepository
{
    private readonly List<Court> _courts = [];

    public Task AddAsync(Court court, CancellationToken cancellationToken = default)
    {
        _courts.Add(court);
        return Task.CompletedTask;
    }

    public Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_courts.FirstOrDefault(c => c.Id == id));
    }

    public Task<Court?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_courts.FirstOrDefault(c => c.Id == id && c.Status == CourtStatus.Active));
    }

    public Task<(IReadOnlyList<Court> Items, int TotalItems)> GetActiveCourtsByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? sportId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _courts
            .Where(c => c.SportsComplexId == sportsComplexId && c.Status == CourtStatus.Active)
            .ToList();

        return Task.FromResult((query as IReadOnlyList<Court>, query.Count));
    }

    public Task<(IReadOnlyList<Court> Items, int TotalItems)> GetCourtsByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? sportId = null,
        CourtStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _courts
            .Where(c => c.SportsComplexId == sportsComplexId)
            .ToList();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value).ToList();
        }

        return Task.FromResult((query as IReadOnlyList<Court>, query.Count));
    }

    public Task<(int ActiveCount, int InactiveCount)> GetCourtStatusCountsByComplexIdAsync(
        Guid sportsComplexId,
        CancellationToken cancellationToken = default)
    {
        var activeCount = _courts.Count(c => c.SportsComplexId == sportsComplexId && c.Status == CourtStatus.Active);
        var inactiveCount = _courts.Count(c => c.SportsComplexId == sportsComplexId && c.Status == CourtStatus.Inactive);

        return Task.FromResult((activeCount, inactiveCount));
    }

    public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_courts.Any(c => c.SportsComplexId == sportsComplexId && c.Name == name));
    }

    public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, Guid excludeCourtId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_courts.Any(c => c.SportsComplexId == sportsComplexId && c.Id != excludeCourtId && c.Name == name));
    }
}
