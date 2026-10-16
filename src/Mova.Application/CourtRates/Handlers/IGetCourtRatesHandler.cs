using Mova.Application.CourtRates.Queries;
using Mova.Contracts.CourtRates;

namespace Mova.Application.CourtRates.Handlers;

public interface IGetCourtRatesHandler
{
    Task<IReadOnlyCollection<CourtRateInfo>> HandleAsync(GetCourtRatesQuery query, CancellationToken cancellationToken = default);
}
