using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IBusinessHoursRepository
{
    Task<IReadOnlyCollection<BusinessHours>> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default);
    Task AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<BusinessHours> businessHours);
}
