using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Validators;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateCourtAvailabilityRulesHandlerTests
{
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeCurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateCourtAvailabilityRulesHandler CreateHandler(ICourtRepository courts, ICourtAvailabilityRuleRepository? rules = null) =>
        new(courts, rules ?? new FakeCourtAvailabilityRuleRepository(), _auditLogs, _currentUser, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidData_UpdatesRulesAndCreatesAuditLog()
    {
        var complexId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.UserId = userId;
        var court = Court.Create(complexId, "Court", "Description", "Synthetic", false);
        var handler = CreateHandler(new FakeCourtRepository(court));

        var result = await handler.HandleAsync(new UpdateCourtAvailabilityRulesCommand(complexId, court.Id,
        [
            new CourtAvailabilityRuleItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true)
        ]));

        var rule = Assert.Single(result);
        Assert.Equal(DayOfWeek.Monday, rule.DayOfWeek);
        Assert.Equal(TimeSpan.FromHours(8), rule.StartTime);
        Assert.Equal(TimeSpan.FromHours(12), rule.EndTime);

        var auditLog = Assert.Single(_auditLogs.AuditLogs);
        Assert.Equal("CourtAvailabilityRule.Update", auditLog.Action);
        Assert.Equal("CourtAvailabilityRule", auditLog.EntityType);
        Assert.Equal(court.Id.ToString(), auditLog.EntityId);
        Assert.Equal(complexId, auditLog.SportsComplexId);
        Assert.Equal(userId, auditLog.UserId);
    }

    [Fact]
    public async Task Handle_WithUnknownCourt_ThrowsNotFound()
    {
        var handler = CreateHandler(new FakeCourtRepository(null));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(
            new UpdateCourtAvailabilityRulesCommand(Guid.NewGuid(), Guid.NewGuid(),
            [
                new CourtAvailabilityRuleItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true)
            ])));
    }

    [Fact]
    public async Task Handle_WithDifferentComplex_ThrowsNotFound()
    {
        var court = Court.Create(Guid.NewGuid(), "Court", "Description", "Synthetic", false);
        var handler = CreateHandler(new FakeCourtRepository(court));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(
            new UpdateCourtAvailabilityRulesCommand(Guid.NewGuid(), court.Id,
            [
                new CourtAvailabilityRuleItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true)
            ])));
    }

    private sealed class FakeCourtRepository(Court? court) : ICourtRepository
    {
        public Task AddAsync(Court court, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Court?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(court is not null && id == court.Id ? court : null);
        public Task<Court?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(court is not null && id == court.Id && court.Status == CourtStatus.Active ? court : null);
        public Task<(IReadOnlyList<Court> Items, int TotalItems)> GetActiveCourtsByComplexIdAsync(Guid sportsComplexId, int page, int pageSize, Guid? sportId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Court> Items, int TotalItems)>(([], 0));
        public Task<(IReadOnlyList<Court> Items, int TotalItems)> GetCourtsByComplexIdAsync(Guid sportsComplexId, int page, int pageSize, Guid? sportId = null, CourtStatus? status = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Court> Items, int TotalItems)>(([], 0));
        public Task<(int ActiveCount, int InactiveCount)> GetCourtStatusCountsByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default) =>
            Task.FromResult((0, 0));
        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> ExistsByNameAsync(Guid sportsComplexId, string name, Guid excludeCourtId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }

    private sealed class FakeCourtAvailabilityRuleRepository : ICourtAvailabilityRuleRepository
    {
        private readonly List<CourtAvailabilityRule> _rules = [];

        public Task<IReadOnlyCollection<CourtAvailabilityRule>> GetByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<CourtAvailabilityRule>>(_rules.Where(r => r.CourtId == courtId).ToArray());

        public Task AddAsync(CourtAvailabilityRule rule, CancellationToken cancellationToken = default)
        {
            _rules.Add(rule);
            return Task.CompletedTask;
        }

        public void RemoveRange(IEnumerable<CourtAvailabilityRule> rules)
        {
            foreach (var rule in rules)
            {
                _rules.Remove(rule);
            }
        }
    }
}
