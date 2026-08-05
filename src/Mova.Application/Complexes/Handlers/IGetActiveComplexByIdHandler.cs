using Mova.Application.Complexes.Queries;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IGetActiveComplexByIdHandler
{
    Task<SportsComplexInfo?> HandleAsync(GetActiveComplexByIdQuery query, CancellationToken cancellationToken = default);
}
