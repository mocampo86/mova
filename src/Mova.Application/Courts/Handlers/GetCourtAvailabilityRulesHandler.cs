using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class GetCourtAvailabilityRulesHandler(
    ICourtRepository courts,
    ICourtAvailabilityRuleRepository rules) : IGetCourtAvailabilityRulesHandler
{
    public async Task<IReadOnlyCollection<CourtAvailabilityRuleInfo>> HandleAsync(GetCourtAvailabilityRulesQuery query, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(query.CourtId, cancellationToken)
            ?? throw new NotFoundException("Court not found.");
        if (court.SportsComplexId != query.SportsComplexId)
            throw new NotFoundException("Court not found.");

        var result = await rules.GetByCourtIdAsync(query.CourtId, cancellationToken);
        return result.Select(CourtAvailabilityRuleMapper.ToInfo).ToArray();
    }
}
