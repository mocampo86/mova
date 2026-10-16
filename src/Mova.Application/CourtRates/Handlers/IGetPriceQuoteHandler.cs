using Mova.Application.CourtRates.Queries;
using Mova.Contracts.CourtRates;

namespace Mova.Application.CourtRates.Handlers;

public interface IGetPriceQuoteHandler
{
    Task<PriceQuoteInfo> HandleAsync(GetPriceQuoteQuery query, CancellationToken cancellationToken = default);
}
