using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Authentication;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateBusinessHoursHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_UpdatesHours()
    {
        var complexId = Guid.NewGuid();
        var repository = new FakeBusinessHoursRepository();
        var handler = new UpdateBusinessHoursHandler(repository, new FakeUnitOfWork());

        var result = await handler.HandleAsync(new UpdateBusinessHoursCommand(complexId,
        [
            new BusinessHoursItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false)
        ]));

        var hours = Assert.Single(result);
        Assert.Equal(complexId, hours.SportsComplexId);
        Assert.Equal(DayOfWeek.Monday, hours.DayOfWeek);
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
