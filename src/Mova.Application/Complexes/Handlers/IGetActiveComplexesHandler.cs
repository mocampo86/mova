using Mova.Application.Complexes.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IGetActiveComplexesHandler
{
    Task<PagedResult<SportsComplexInfo>> HandleAsync(GetActiveComplexesQuery query, CancellationToken cancellationToken = default);
}
