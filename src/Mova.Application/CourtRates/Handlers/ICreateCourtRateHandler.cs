using Mova.Application.CourtRates.Commands;
using Mova.Contracts.CourtRates;

namespace Mova.Application.CourtRates.Handlers;

public interface ICreateCourtRateHandler
{
    Task<CourtRateInfo> HandleAsync(CreateCourtRateCommand command, CancellationToken cancellationToken = default);
}
