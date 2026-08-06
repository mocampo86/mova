using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class BusinessHoursRepository(MovaDbContext context) : IBusinessHoursRepository
{
    public async Task<IReadOnlyCollection<BusinessHours>> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default) =>
        await context.BusinessHours.Where(x => x.SportsComplexId == sportsComplexId).ToArrayAsync(cancellationToken);

    public Task AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default) =>
        context.BusinessHours.AddAsync(businessHours, cancellationToken).AsTask();

    public void RemoveRange(IEnumerable<BusinessHours> businessHours) =>
        context.BusinessHours.RemoveRange(businessHours);
}
