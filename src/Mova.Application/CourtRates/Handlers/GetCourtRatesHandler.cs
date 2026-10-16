using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.CourtRates.Queries;
using Mova.Contracts.CourtRates;

namespace Mova.Application.CourtRates.Handlers;

public sealed class GetCourtRatesHandler(
    ICourtRepository courts,
    ICourtRateRepository rates) : IGetCourtRatesHandler
{
    public async Task<IReadOnlyCollection<CourtRateInfo>> HandleAsync(GetCourtRatesQuery query, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(query.CourtId, cancellationToken);
        if (court is null || court.SportsComplexId != query.SportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        var courtRates = await rates.GetByCourtIdAsync(query.CourtId, cancellationToken);

        return courtRates
            .OrderBy(r => r.StartTime)
            .ThenBy(r => r.DaysOfWeek)
            .Select(CourtRateMapper.ToInfo)
            .ToArray();
    }
}
