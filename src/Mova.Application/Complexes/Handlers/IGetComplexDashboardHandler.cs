using Mova.Application.Complexes.Queries;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IGetComplexDashboardHandler
{
    Task<ComplexDashboardInfo?> HandleAsync(GetComplexDashboardQuery query, CancellationToken cancellationToken = default);
}
