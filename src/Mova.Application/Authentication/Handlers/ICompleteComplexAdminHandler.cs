using Mova.Application.Authentication.Commands;
using Mova.Contracts.Auth;

namespace Mova.Application.Authentication.Handlers;

public interface ICompleteComplexAdminHandler
{
    Task<CompleteComplexAdminResponse> HandleAsync(
        CompleteComplexAdminCommand command,
        CancellationToken cancellationToken = default);
}
