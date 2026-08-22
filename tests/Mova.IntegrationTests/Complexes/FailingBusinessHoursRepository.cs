using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.IntegrationTests.Complexes;

public sealed class FailingBusinessHoursRepository : IBusinessHoursRepository
{
    private readonly IBusinessHoursRepository _inner;
    private readonly int _failAfter;
    private int _callCount;

    public FailingBusinessHoursRepository(IBusinessHoursRepository inner, int failAfter)
    {
        _inner = inner;
        _failAfter = failAfter;
    }

    public Task<IReadOnlyCollection<BusinessHours>> GetBySportsComplexIdAsync(
        Guid sportsComplexId,
        CancellationToken cancellationToken = default)
    {
        return _inner.GetBySportsComplexIdAsync(sportsComplexId, cancellationToken);
    }

    public async Task AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default)
    {
        _callCount++;

        if (_callCount > _failAfter)
        {
            throw new InvalidOperationException("Simulated business-hours persistence failure.");
        }

        await _inner.AddAsync(businessHours, cancellationToken);
    }

    public void RemoveRange(IEnumerable<BusinessHours> businessHours)
    {
        _inner.RemoveRange(businessHours);
    }
}
