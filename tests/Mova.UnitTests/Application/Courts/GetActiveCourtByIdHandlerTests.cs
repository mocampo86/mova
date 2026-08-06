using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Courts;

public sealed class GetActiveCourtByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithActiveCourt_ReturnsCourtInfo()
    {
        var court = Court.Create(Guid.NewGuid(), "Court", "Description", "Surface", false);
        var handler = new GetActiveCourtByIdHandler(new FakeCourtRepository([court]));

        var result = await handler.HandleAsync(new GetActiveCourtByIdQuery(court.Id));

        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCourt_ReturnsNull()
    {
        var court = Court.Create(Guid.NewGuid(), "Court", "Description", "Surface", false);
        court.Deactivate();
        var handler = new GetActiveCourtByIdHandler(new FakeCourtRepository([court]));

        var result = await handler.HandleAsync(new GetActiveCourtByIdQuery(court.Id));

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCourt_ReturnsNull()
    {
        var handler = new GetActiveCourtByIdHandler(new FakeCourtRepository([]));

        var result = await handler.HandleAsync(new GetActiveCourtByIdQuery(Guid.NewGuid()));

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
            CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Court> Items, int TotalItems)>(([], 0));

        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
