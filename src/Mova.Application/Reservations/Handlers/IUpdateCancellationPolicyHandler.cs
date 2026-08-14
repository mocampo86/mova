using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;

namespace Mova.Application.Reservations.Handlers;

public interface IUpdateCancellationPolicyHandler
{
    Task<CancellationPolicyInfo> HandleAsync(UpdateCancellationPolicyCommand command, CancellationToken cancellationToken = default);
}
