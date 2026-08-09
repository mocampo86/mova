using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IUpdateCourtHandler
{
    Task<CourtInfo> HandleAsync(UpdateCourtCommand command, CancellationToken cancellationToken = default);
}
