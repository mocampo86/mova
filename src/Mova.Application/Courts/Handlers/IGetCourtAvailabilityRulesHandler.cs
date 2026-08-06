using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetCourtAvailabilityRulesHandler
{
    Task<IReadOnlyCollection<CourtAvailabilityRuleInfo>> HandleAsync(GetCourtAvailabilityRulesQuery query, CancellationToken cancellationToken = default);
}
