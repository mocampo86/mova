using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetActiveSportsHandler
{
    Task<IReadOnlyCollection<SportInfo>> HandleAsync(GetActiveSportsQuery query, CancellationToken cancellationToken = default);
}
