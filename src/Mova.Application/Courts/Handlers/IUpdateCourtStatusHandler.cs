using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IUpdateCourtStatusHandler
{
    Task<CourtInfo> HandleAsync(UpdateCourtStatusCommand command, CancellationToken cancellationToken = default);
}
