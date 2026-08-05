using Mova.Application.Complexes.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IGetAllComplexesHandler
{
    Task<PagedResult<SportsComplexInfo>> HandleAsync(GetAllComplexesQuery query, CancellationToken cancellationToken = default);
}
