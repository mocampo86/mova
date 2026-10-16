using Mova.Application.Common.Exceptions;
using Mova.Application.CourtRates.Commands;
using Mova.Application.CourtRates.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Exceptions;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.CourtRates;

public sealed class CourtRateHandlerTests
{
    private readonly FakeCourtRepository _courtRepository = new();
    private readonly FakeCourtRateRepository _rateRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeCurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateCourtRateHandler CreateHandler() =>
        new(_courtRepository, _rateRepository, _auditLogs, _currentUser, _unitOfWork);

    private UpdateCourtRateHandler CreateUpdateHandler() =>
        new(_courtRepository, _rateRepository, _auditLogs, _currentUser, _unitOfWork);

    private async Task<(SportsComplex Complex, Court Court)> CreateScenarioAsync()
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

        var court = Court.Create(complex.Id, "Court 1", "Description", "Synthetic", false);
        await _courtRepository.AddAsync(court);

        return (complex, court);
    }

    [Fact]
    public async Task Create_WithValidData_CreatesRateAndAuditLog()
    {
        var (complex, court) = await CreateScenarioAsync();
        var adminUserId = Guid.NewGuid();
        _currentUser.UserId = adminUserId;
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new CreateCourtRateCommand(
            complex.Id,
            court.Id,
            [DayOfWeek.Monday, DayOfWeek.Friday],
            new TimeSpan(17, 0, 0),
            new TimeSpan(22, 0, 0),
            1200m));

        Assert.Equal(court.Id, result.CourtId);
        Assert.Equal([1, 5], result.DaysOfWeek);
        Assert.Equal(1200m, result.AmountPerHour);
        Assert.True(result.IsActive);

        var auditLog = Assert.Single(_auditLogs.AuditLogs);
        Assert.Equal("CourtRate.Create", auditLog.Action);
        Assert.Equal(adminUserId, auditLog.UserId);
    }

    [Fact]
    public async Task Create_WithOverlappingActiveRate_ThrowsRateOverlapException()
    {
        var (complex, court) = await CreateScenarioAsync();
        await _rateRepository.AddAsync(CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(19),
            1200m));
        var handler = CreateHandler();

        await Assert.ThrowsAsync<RateOverlapException>(() => handler.HandleAsync(new CreateCourtRateCommand(
            complex.Id,
            court.Id,
            [DayOfWeek.Monday, DayOfWeek.Wednesday],
            new TimeSpan(18, 0, 0),
            new TimeSpan(20, 0, 0),
            1500m)));
    }

    [Fact]
    public async Task Create_WithOverlappingInactiveRate_AllowsCreation()
    {
        var (complex, court) = await CreateScenarioAsync();
        var existing = CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(19),
            1200m);
        existing.Deactivate();
        await _rateRepository.AddAsync(existing);
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new CreateCourtRateCommand(
            complex.Id,
            court.Id,
            [DayOfWeek.Monday],
            new TimeSpan(18, 0, 0),
            new TimeSpan(20, 0, 0),
            1500m));

        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Create_WithAdjacentRate_AllowsCreation()
    {
        var (complex, court) = await CreateScenarioAsync();
        await _rateRepository.AddAsync(CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(18),
            1200m));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new CreateCourtRateCommand(
            complex.Id,
            court.Id,
            [DayOfWeek.Monday],
            new TimeSpan(18, 0, 0),
            new TimeSpan(19, 0, 0),
            1800m));

        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Create_WithCourtFromAnotherComplex_ThrowsNotFoundException()
    {
        var (complex, court) = await CreateScenarioAsync();
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CreateCourtRateCommand(
            Guid.NewGuid(),
            court.Id,
            [DayOfWeek.Monday],
            new TimeSpan(18, 0, 0),
            new TimeSpan(20, 0, 0),
            1500m)));
    }

    [Fact]
    public async Task Update_WithOverlapAgainstItself_IsAllowed()
    {
        var (complex, court) = await CreateScenarioAsync();
        var rate = CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(19),
            1200m);
        await _rateRepository.AddAsync(rate);
        var handler = CreateUpdateHandler();

        var result = await handler.HandleAsync(new UpdateCourtRateCommand(
            complex.Id,
            court.Id,
            rate.Id,
            [DayOfWeek.Monday],
            new TimeSpan(17, 0, 0),
            new TimeSpan(19, 0, 0),
            1400m));

        Assert.Equal(1400m, result.AmountPerHour);
    }

    [Fact]
    public async Task Update_WithOverlapAgainstAnotherRate_ThrowsRateOverlapException()
    {
        var (complex, court) = await CreateScenarioAsync();
        var rate = CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(19),
            1200m);
        var other = CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(19),
            TimeSpan.FromHours(21),
            1200m);
        await _rateRepository.AddAsync(rate);
        await _rateRepository.AddAsync(other);
        var handler = CreateUpdateHandler();

        await Assert.ThrowsAsync<RateOverlapException>(() => handler.HandleAsync(new UpdateCourtRateCommand(
            complex.Id,
            court.Id,
            rate.Id,
            [DayOfWeek.Monday],
            new TimeSpan(18, 0, 0),
            new TimeSpan(20, 0, 0),
            1200m)));
    }

    [Fact]
    public async Task Update_OfInactiveRate_SkipsOverlapCheck()
    {
        var (complex, court) = await CreateScenarioAsync();
        var rate = CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(19),
            1200m);
        rate.Deactivate();
        var other = CourtRate.Create(
            court.Id,
            1 << (int)DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(19),
            1500m);
        await _rateRepository.AddAsync(rate);
        await _rateRepository.AddAsync(other);
        var handler = CreateUpdateHandler();

        var result = await handler.HandleAsync(new UpdateCourtRateCommand(
            complex.Id,
            court.Id,
            rate.Id,
            [DayOfWeek.Monday],
            new TimeSpan(17, 0, 0),
            new TimeSpan(19, 0, 0),
            1400m));

        Assert.Equal(1400m, result.AmountPerHour);
        Assert.False(result.IsActive);
    }
}
