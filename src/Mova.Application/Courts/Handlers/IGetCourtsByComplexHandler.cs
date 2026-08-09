using Mova.Application.Courts.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetCourtsByComplexHandler
{
    Task<PagedResult<CourtInfo>> HandleAsync(GetCourtsByComplexQuery query, CancellationToken cancellationToken = default);
}
