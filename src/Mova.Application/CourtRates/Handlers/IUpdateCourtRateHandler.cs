using Mova.Application.CourtRates.Commands;
using Mova.Contracts.CourtRates;

namespace Mova.Application.CourtRates.Handlers;

public interface IUpdateCourtRateHandler
{
    Task<CourtRateInfo> HandleAsync(UpdateCourtRateCommand command, CancellationToken cancellationToken = default);
}
