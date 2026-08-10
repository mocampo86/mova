using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class GetCourtByIdHandler(ICourtRepository courts) : IGetCourtByIdHandler
{
    public async Task<CourtInfo?> HandleAsync(GetCourtByIdQuery query, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(query.CourtId, cancellationToken);
        if (court is null || court.SportsComplexId != query.SportsComplexId)
            return null;

        return CourtMapper.ToInfo(court);
    }
}
