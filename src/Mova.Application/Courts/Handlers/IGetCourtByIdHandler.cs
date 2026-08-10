using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetCourtByIdHandler
{
    Task<CourtInfo?> HandleAsync(GetCourtByIdQuery query, CancellationToken cancellationToken = default);
}
