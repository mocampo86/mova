using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Courts;

public sealed class GetActiveCourtsByComplexHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithActiveAndInactiveCourts_ReturnsOnlyActive()
    {
        var complexId = Guid.NewGuid();
        var activeCourt = Court.Create(complexId, "Active Court", "Description", "Surface", false);
        var inactiveCourt = Court.Create(complexId, "Inactive Court", "Description", "Surface", false);
        inactiveCourt.Deactivate();
        var handler = new GetActiveCourtsByComplexHandler(new FakeCourtRepository([activeCourt, inactiveCourt]));

        var result = await handler.HandleAsync(new GetActiveCourtsByComplexQuery(complexId, 1, 20));

        Assert.Single(result.Items);
        Assert.Equal(activeCourt.Id, result.Items[0].Id);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task HandleAsync_WithNoCourts_ReturnsEmptyPagedResult()
    {
        var handler = new GetActiveCourtsByComplexHandler(new FakeCourtRepository([]));

        var result = await handler.HandleAsync(new GetActiveCourtsByComplexQuery(Guid.NewGuid(), 1, 20));

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    private sealed class FakeCourtRepository(IEnumerable<Court> courts) : ICourtRepository
    {
        private readonly IReadOnlyList<Court> _courts = courts.ToList();

        public Task AddAsync(Court court, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_courts.FirstOrDefault(c => c.Id == id));

        public Task<Court?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_courts.FirstOrDefault(c => c.Id == id && c.Status == CourtStatus.Active));

        public Task<(IReadOnlyList<Court> Items, int TotalItems)> GetActiveCourtsByComplexIdAsync(
            Guid sportsComplexId,
            int page,
            int pageSize,
            Guid? sportId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _courts
                .Where(c => c.SportsComplexId == sportsComplexId && c.Status == CourtStatus.Active)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 1 : pageSize;

            var totalItems = query.Count;
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<(IReadOnlyList<Court> Items, int TotalItems)>((items, totalItems));
        }

        public Task<(int ActiveCount, int InactiveCount)> GetCourtStatusCountsByComplexIdAsync(
            Guid sportsComplexId,
            CancellationToken cancellationToken = default)
        {
            var activeCount = _courts.Count(c => c.SportsComplexId == sportsComplexId && c.Status == CourtStatus.Active);
            var inactiveCount = _courts.Count(c => c.SportsComplexId == sportsComplexId && c.Status == CourtStatus.Inactive);
            return Task.FromResult((activeCount, inactiveCount));
        }

        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
