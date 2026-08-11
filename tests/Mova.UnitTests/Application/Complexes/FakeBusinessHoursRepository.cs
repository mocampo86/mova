using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeBusinessHoursRepository : IBusinessHoursRepository
{
    private readonly List<BusinessHours> _businessHours = [];

    public Task AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default)
    {
        _businessHours.Add(businessHours);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<BusinessHours>> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<BusinessHours>>(_businessHours.Where(x => x.SportsComplexId == sportsComplexId).ToList());
    }

    public void RemoveRange(IEnumerable<BusinessHours> businessHours)
    {
        foreach (var item in businessHours)
        {
            _businessHours.Remove(item);
        }
    }
}
