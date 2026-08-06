using Mova.Contracts.Courts;
using Mova.Application.Courts.Commands;

namespace Mova.Application.Courts.Handlers;

public interface ICreateCourtHandler
{
    Task<CourtInfo> HandleAsync(CreateCourtCommand command, CancellationToken cancellationToken = default);
}
