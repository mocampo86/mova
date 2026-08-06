using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class GetBusinessHoursHandler(
    IBusinessHoursRepository businessHours) : IGetBusinessHoursHandler
{
    public async Task<IReadOnlyCollection<BusinessHoursInfo>> HandleAsync(GetBusinessHoursQuery query, CancellationToken cancellationToken = default)
    {
        var result = await businessHours.GetBySportsComplexIdAsync(query.SportsComplexId, cancellationToken);
        return result.Select(BusinessHoursMapper.ToInfo).ToArray();
    }
}
