using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateCourtStatusHandlerTests
{
    private readonly FakeCourtRepository _courtRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateCourtStatusHandler CreateHandler() =>
        new(_courtRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithActiveCourt_DeactivatesAndReturnsInfo()
    {
        var complexId = Guid.NewGuid();
        var court = Court.Create(complexId, "Court", "Description", "Surface", false);
        _courtRepository.Add(court);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateCourtStatusCommand(complexId, court.Id, Guid.NewGuid(), CourtStatus.Inactive));

        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
        Assert.Equal("Inactive", result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCourt_ActivatesAndReturnsInfo()
    {
        var complexId = Guid.NewGuid();
        var court = Court.Create(complexId, "Court", "Description", "Surface", false);
        court.Deactivate();
        _courtRepository.Add(court);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateCourtStatusCommand(complexId, court.Id, Guid.NewGuid(), CourtStatus.Active));

        Assert.NotNull(result);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCourt_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateCourtStatusCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CourtStatus.Inactive)));
    }

    [Fact]
    public async Task HandleAsync_WithCourtFromDifferentComplex_ThrowsNotFoundException()
    {
        var court = Court.Create(Guid.NewGuid(), "Court", "Description", "Surface", false);
        _courtRepository.Add(court);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateCourtStatusCommand(
            Guid.NewGuid(),
            court.Id,
            Guid.NewGuid(),
            CourtStatus.Inactive)));
    }

    private sealed class FakeCourtRepository : ICourtRepository
    {
        private readonly List<Court> _courts = [];

        public void Add(Court court) => _courts.Add(court);

        public Task AddAsync(Court court, CancellationToken cancellationToken = default)
        {
            _courts.Add(court);
            return Task.CompletedTask;
        }

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
            var query = _courts.Where(c => c.SportsComplexId == sportsComplexId && c.Status == CourtStatus.Active).ToList();
            return Task.FromResult<(IReadOnlyList<Court> Items, int TotalItems)>((query, query.Count));
        }

        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
