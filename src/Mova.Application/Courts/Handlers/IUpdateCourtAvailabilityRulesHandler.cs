using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IUpdateCourtAvailabilityRulesHandler
{
    Task<IReadOnlyCollection<CourtAvailabilityRuleInfo>> HandleAsync(UpdateCourtAvailabilityRulesCommand command, CancellationToken cancellationToken = default);
}
