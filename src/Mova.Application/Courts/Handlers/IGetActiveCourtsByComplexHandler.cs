using Mova.Application.Courts.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetActiveCourtsByComplexHandler
{
    Task<PagedResult<CourtInfo>> HandleAsync(GetActiveCourtsByComplexQuery query, CancellationToken cancellationToken = default);
}
