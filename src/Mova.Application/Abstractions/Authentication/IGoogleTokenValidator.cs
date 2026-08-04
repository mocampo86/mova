using Mova.Application.Authentication.Models;

namespace Mova.Application.Abstractions.Authentication;

public interface IGoogleTokenValidator
{
    Task<GoogleTokenValidationResult> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
