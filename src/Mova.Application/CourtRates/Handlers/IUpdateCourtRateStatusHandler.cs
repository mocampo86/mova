using Mova.Application.CourtRates.Commands;
using Mova.Contracts.CourtRates;

namespace Mova.Application.CourtRates.Handlers;

public interface IUpdateCourtRateStatusHandler
{
    Task<CourtRateInfo> HandleAsync(UpdateCourtRateStatusCommand command, CancellationToken cancellationToken = default);
}
