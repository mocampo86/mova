using Mova.Application.Abstractions.Authentication;
using Mova.Application.Authentication.Models;
using Mova.Application.Common.Exceptions;

namespace Mova.UnitTests.Application.Authentication;

public sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    public Task<GoogleTokenValidationResult> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (idToken == "invalid-token")
        {
            throw new AuthenticationException("Invalid Google token.");
        }

        return Task.FromResult(new GoogleTokenValidationResult("google-subject", "user@test.com", "John Doe"));
    }
}
