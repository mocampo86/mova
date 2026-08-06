using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetActiveCourtByIdHandler
{
    Task<CourtInfo?> HandleAsync(GetActiveCourtByIdQuery query, CancellationToken cancellationToken = default);
}
