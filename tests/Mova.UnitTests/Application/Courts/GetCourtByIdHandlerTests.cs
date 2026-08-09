using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Courts;

public sealed class GetCourtByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingCourt_ReturnsCourtInfo()
    {
        var complexId = Guid.NewGuid();
        var court = Court.Create(complexId, "Court", "Description", "Synthetic", false);
        var handler = new GetCourtByIdHandler(new FakeCourtRepository([court]));

        var result = await handler.HandleAsync(new GetCourtByIdQuery(complexId, court.Id));

        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
    }

    [Fact]
    public async Task HandleAsync_WithCourtFromDifferentComplex_ReturnsNull()
    {
        var court = Court.Create(Guid.NewGuid(), "Court", "Description", "Synthetic", false);
        var handler = new GetCourtByIdHandler(new FakeCourtRepository([court]));

        var result = await handler.HandleAsync(new GetCourtByIdQuery(Guid.NewGuid(), court.Id));

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCourt_ReturnsCourtInfo()
    {
        var complexId = Guid.NewGuid();
        var court = Court.Create(complexId, "Court", "Description", "Synthetic", false);
        court.Deactivate();
        var handler = new GetCourtByIdHandler(new FakeCourtRepository([court]));

        var result = await handler.HandleAsync(new GetCourtByIdQuery(complexId, court.Id));

        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCourt_ReturnsNull()
    {
        var handler = new GetCourtByIdHandler(new FakeCourtRepository([]));

        var result = await handler.HandleAsync(new GetCourtByIdQuery(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Null(result);
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
            CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Court> Items, int TotalItems)>(([], 0));

        public Task<(IReadOnlyList<Court> Items, int TotalItems)> GetCourtsByComplexIdAsync(
            Guid sportsComplexId,
            int page,
            int pageSize,
            Guid? sportId = null,
            CourtStatus? status = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Court> Items, int TotalItems)>(([], 0));

        public Task<(int ActiveCount, int InactiveCount)> GetCourtStatusCountsByComplexIdAsync(
            Guid sportsComplexId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult((0, 0));

        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, Guid excludeCourtId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
