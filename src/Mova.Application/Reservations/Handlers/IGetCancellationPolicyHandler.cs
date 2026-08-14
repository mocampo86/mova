using Mova.Application.Reservations.Queries;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IGetCancellationPolicyHandler
{
    Task<CancellationPolicyInfo> HandleAsync(GetCancellationPolicyQuery query, CancellationToken cancellationToken = default);
}
