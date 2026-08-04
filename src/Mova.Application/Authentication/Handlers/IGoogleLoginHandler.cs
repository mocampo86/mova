using Mova.Application.Authentication.Commands;
using Mova.Contracts.Auth;

namespace Mova.Application.Authentication.Handlers;

public interface IGoogleLoginHandler
{
    Task<GoogleLoginResponse> HandleAsync(GoogleLoginCommand command, CancellationToken cancellationToken = default);
}
