using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateCourtHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeCourtRepository _courtRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeCurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateCourtHandler CreateHandler(ISportRepository? sports = null) =>
        new(_sportsComplexRepository, _courtRepository, sports ?? new FakeSportRepository([]), _auditLogs, _currentUser, _unitOfWork);

    private async Task<SportsComplex> CreateComplexAsync()
    {
        var complex = SportsComplex.Create(
            "Test Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            $"test-{Guid.NewGuid()}@example.com");
        await _sportsComplexRepository.AddAsync(complex);
        return complex;
    }

    [Fact]
    public async Task HandleAsync_WithValidData_UpdatesCourtAndReturnsInfo()
    {
        var complex = await CreateComplexAsync();
        var court = Court.Create(complex.Id, "Court", "Description", "Synthetic", false);
        await _courtRepository.AddAsync(court);
        var football = Sport.Create("Football");
        var handler = CreateHandler(new FakeSportRepository([football]));

        var result = await handler.HandleAsync(new UpdateCourtCommand(
            complex.Id,
            court.Id,
            "Updated Court",
            "Updated description",
            "Grass",
            true,
            [football.Id]));

        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
        Assert.Equal("Updated Court", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("Grass", result.SurfaceType);
        Assert.True(result.Indoor);
        Assert.Single(result.SportIds);
        Assert.Equal(football.Id, result.SportIds.Single());
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WithSameName_DoesNotConflict()
    {
        var complex = await CreateComplexAsync();
        var court = Court.Create(complex.Id, "Court", "Description", "Synthetic", false);
        await _courtRepository.AddAsync(court);
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new UpdateCourtCommand(
            complex.Id,
            court.Id,
            "Court",
            "Updated description",
            "Synthetic",
            false,
            null));

        Assert.NotNull(result);
        Assert.Equal("Court", result.Name);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_ThrowsConflictException()
    {
        var complex = await CreateComplexAsync();
        var firstCourt = Court.Create(complex.Id, "First Court", "Description", "Synthetic", false);
        var secondCourt = Court.Create(complex.Id, "Second Court", "Description", "Synthetic", false);
        await _courtRepository.AddAsync(firstCourt);
        await _courtRepository.AddAsync(secondCourt);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(new UpdateCourtCommand(
            complex.Id,
            firstCourt.Id,
            "Second Court",
            "Updated description",
            "Synthetic",
            false,
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateCourtCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Court",
            "Description",
            "Synthetic",
            false,
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentCourt_ThrowsNotFoundException()
    {
        var complex = await CreateComplexAsync();
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateCourtCommand(
            complex.Id,
            Guid.NewGuid(),
            "Court",
            "Description",
            "Synthetic",
            false,
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithCourtFromDifferentComplex_ThrowsNotFoundException()
    {
        var firstComplex = await CreateComplexAsync();
        var secondComplex = await CreateComplexAsync();
        var court = Court.Create(firstComplex.Id, "Court", "Description", "Synthetic", false);
        await _courtRepository.AddAsync(court);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateCourtCommand(
            secondComplex.Id,
            court.Id,
            "Court",
            "Description",
            "Synthetic",
            false,
            null)));
    }

    [Fact]
    public async Task HandleAsync_WithUnknownSport_ThrowsNotFoundException()
    {
        var complex = await CreateComplexAsync();
        var court = Court.Create(complex.Id, "Court", "Description", "Synthetic", false);
        await _courtRepository.AddAsync(court);
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateCourtCommand(
            complex.Id,
            court.Id,
            "Court",
            "Description",
            "Synthetic",
            false,
            [Guid.NewGuid()])));
    }

    [Fact]
    public async Task HandleAsync_WithEmptySports_ClearsSports()
    {
        var complex = await CreateComplexAsync();
        var football = Sport.Create("Football");
        var court = Court.Create(complex.Id, "Court", "Description", "Synthetic", false, [football.Id]);
        await _courtRepository.AddAsync(court);
        var handler = CreateHandler(new FakeSportRepository([football]));

        var result = await handler.HandleAsync(new UpdateCourtCommand(
            complex.Id,
            court.Id,
            "Court",
            "Description",
            "Synthetic",
            false,
            []));

        Assert.NotNull(result);
        Assert.Empty(result.SportIds);
    }

    private sealed class FakeSportRepository(IEnumerable<Sport> sports) : ISportRepository
    {
        private readonly IReadOnlyCollection<Sport> _sports = sports.ToArray();

        public Task<IReadOnlyCollection<Sport>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Sport>>(_sports.Where(x => ids.Contains(x.Id)).ToArray());

        public Task<IReadOnlyCollection<Sport>> GetActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Sport>>(_sports.Where(x => x.Status == SportStatus.Active).ToArray());
    }
}
