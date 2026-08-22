using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Complexes;

public sealed class ThrowingBusinessHoursRepository : IBusinessHoursRepository
{
    public Task AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated business-hours failure.");
    }

    public Task<IReadOnlyCollection<BusinessHours>> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<BusinessHours>>([]);
    }

    public void RemoveRange(IEnumerable<BusinessHours> businessHours)
    {
    }
}
