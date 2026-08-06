using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class GetActiveSportsHandler(ISportRepository sports) : IGetActiveSportsHandler
{
    public async Task<IReadOnlyCollection<SportInfo>> HandleAsync(GetActiveSportsQuery query, CancellationToken cancellationToken = default)
    {
        var activeSports = await sports.GetActiveAsync(cancellationToken);
        return activeSports.Select(SportMapper.ToInfo).ToArray();
    }
}
