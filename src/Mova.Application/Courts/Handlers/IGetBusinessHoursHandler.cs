using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetBusinessHoursHandler
{
    Task<IReadOnlyCollection<BusinessHoursInfo>> HandleAsync(GetBusinessHoursQuery query, CancellationToken cancellationToken = default);
}
