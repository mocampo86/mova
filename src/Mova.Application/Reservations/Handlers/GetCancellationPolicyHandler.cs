using Mova.Application.Abstractions.Persistence;
using Mova.Application.Abstractions.Policies;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public sealed class GetCancellationPolicyHandler(
    ISportsComplexRepository sportsComplexes,
    ICancellationPolicy cancellationPolicy) : IGetCancellationPolicyHandler
{
    public async Task<CancellationPolicyInfo> HandleAsync(GetCancellationPolicyQuery query, CancellationToken cancellationToken = default)
    {
        var complex = await sportsComplexes.GetByIdAsync(query.SportsComplexId, cancellationToken);

        if (complex is null)
        {
            throw new NotFoundException("Sports complex not found.");
        }

        var settings = await cancellationPolicy.GetSettingsAsync(query.SportsComplexId, cancellationToken);

        return new CancellationPolicyInfo
        {
            SportsComplexId = query.SportsComplexId,
            MinimumHours = settings.MinimumHours,
            AllowUserCancellation = settings.AllowUserCancellation
        };
    }
}
