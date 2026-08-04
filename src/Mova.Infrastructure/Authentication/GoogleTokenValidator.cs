using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Mova.Application.Abstractions.Authentication;
using Mova.Application.Authentication.Models;
using Mova.Application.Common.Exceptions;
using Mova.Infrastructure.Authentication.Options;

namespace Mova.Infrastructure.Authentication;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleTokenValidationResult> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new InvalidOperationException("Google ClientId is not configured.");
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Subject))
            {
                throw new AuthenticationException("Google token is missing required claims.");
            }

            return new GoogleTokenValidationResult(payload.Subject, payload.Email, payload.Name ?? string.Empty);
        }
        catch (InvalidJwtException)
        {
            throw new AuthenticationException("Google token is invalid or has expired.");
        }
    }
}
