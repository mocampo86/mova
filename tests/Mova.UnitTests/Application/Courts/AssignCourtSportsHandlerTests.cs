using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Authentication;

namespace Mova.UnitTests.Application.Courts;

public sealed class AssignCourtSportsHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingSports_AssignsThemToCourt()
    {
        var complexId = Guid.NewGuid();
        var court = Court.Create(complexId, "Court", "Description", "Surface", false);
        var sports = new[] { Sport.Create("Football"), Sport.Create("Padel") };
        var handler = new AssignCourtSportsHandler(
            new FakeCourtRepository(court), new FakeSportRepository(sports), new FakeUnitOfWork());

        var result = await handler.HandleAsync(new AssignCourtSportsCommand(complexId, court.Id, sports.Select(x => x.Id).ToArray()));

        Assert.Equal(sports.Select(x => x.Id), result.SportIds);
        Assert.Equal(sports.Select(x => x.Id), court.CourtSports.Select(x => x.SportId));
    }

    [Fact]
    public async Task Handle_WithUnknownSport_ThrowsNotFound()
    {
        var complexId = Guid.NewGuid();
        var court = Court.Create(complexId, "Court", "Description", "Surface", false);
        var handler = new AssignCourtSportsHandler(
            new FakeCourtRepository(court), new FakeSportRepository([]), new FakeUnitOfWork());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(
            new AssignCourtSportsCommand(complexId, court.Id, [Guid.NewGuid()])));
    }

    private sealed class FakeCourtRepository(Court court) : ICourtRepository
    {
        public Task AddAsync(Court court, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == court.Id ? court : null);
        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class FakeSportRepository(IEnumerable<Sport> sports) : ISportRepository
    {
        private readonly IReadOnlyCollection<Sport> _sports = sports.ToArray();
        public Task<IReadOnlyCollection<Sport>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Sport>>(_sports.Where(x => ids.Contains(x.Id)).ToArray());
    }
}
