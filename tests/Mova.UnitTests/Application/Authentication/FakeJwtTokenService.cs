using Mova.Application.Abstractions.Authentication;
using Mova.Application.Authentication.Models;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Authentication;

public sealed class FakeJwtTokenService : IJwtTokenService
{
    public Task<AuthToken> GenerateAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AuthToken("test-access-token", DateTimeOffset.UtcNow.AddMinutes(15)));
    }
}
