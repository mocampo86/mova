using Mova.Application.Abstractions.Authentication;
using Mova.Application.Authentication.Models;
using Mova.Application.Common.Exceptions;

namespace Mova.IntegrationTests.Authentication;

public sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    public Task<GoogleTokenValidationResult> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (idToken == "invalid-token")
        {
            throw new AuthenticationException("Invalid Google token.");
        }

        var suffix = idToken.StartsWith("valid-token-", StringComparison.Ordinal)
            ? idToken[12..]
            : Guid.NewGuid().ToString();

        return Task.FromResult(new GoogleTokenValidationResult(
            $"sub-{suffix}",
            $"user-{suffix}@test.com",
            $"Test User {suffix}"));
    }
}
