using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class GetActiveCourtByIdHandler(
    ICourtRepository courts) : IGetActiveCourtByIdHandler
{
    public async Task<CourtInfo?> HandleAsync(GetActiveCourtByIdQuery query, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetActiveByIdAsync(query.CourtId, cancellationToken);
        return court is null ? null : CourtMapper.ToPublicInfo(court);
    }
}
