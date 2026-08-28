using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateBusinessHoursHandlerTests
{
    private readonly FakeBusinessHoursRepository _repository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeCurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateBusinessHoursHandler CreateHandler() =>
        new(_repository, _auditLogs, _currentUser, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidData_UpdatesHoursAndCreatesAuditLog()
    {
        var complexId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.UserId = userId;
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new UpdateBusinessHoursCommand(complexId,
        [
            new BusinessHoursItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false)
        ]));

        var hours = Assert.Single(result);
        Assert.Equal(complexId, hours.SportsComplexId);
        Assert.Equal(DayOfWeek.Monday, hours.DayOfWeek);

        var auditLog = Assert.Single(_auditLogs.AuditLogs);
        Assert.Equal("BusinessHours.Update", auditLog.Action);
        Assert.Equal("BusinessHours", auditLog.EntityType);
        Assert.Equal(complexId.ToString(), auditLog.EntityId);
        Assert.Equal(complexId, auditLog.SportsComplexId);
        Assert.Equal(userId, auditLog.UserId);
    }

    private sealed class FakeBusinessHoursRepository : IBusinessHoursRepository
    {
        private readonly List<BusinessHours> _hours = [];

        public Task<IReadOnlyCollection<BusinessHours>> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<BusinessHours>>(_hours.Where(h => h.SportsComplexId == sportsComplexId).ToArray());

        public Task AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default)
        {
            _hours.Add(businessHours);
            return Task.CompletedTask;
        }

        public void RemoveRange(IEnumerable<BusinessHours> businessHours)
        {
            foreach (var item in businessHours)
            {
                _hours.Remove(item);
            }
        }
    }
}
