using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IAssignCourtSportsHandler
{
    Task<CourtInfo> HandleAsync(AssignCourtSportsCommand command, CancellationToken cancellationToken = default);
}
