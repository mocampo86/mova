using Mova.Application.Complexes.Queries;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IGetComplexByIdForAdminHandler
{
    Task<SportsComplexInfo?> HandleAsync(GetComplexByIdForAdminQuery query, CancellationToken cancellationToken = default);
}
