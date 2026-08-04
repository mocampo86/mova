using Mova.Application.Abstractions.Authentication;
using Mova.Application.Authentication.Models;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Authentication;

public sealed class FakeJwtTokenService : IJwtTokenService
{
    public IReadOnlyCollection<string>? LastRoles { get; private set; }
    public IReadOnlyCollection<UserComplexAssociation>? LastComplexAssociations { get; private set; }

    public Task<AuthToken> GenerateAsync(
        User user,
        IEnumerable<string> roles,
        IEnumerable<UserComplexAssociation> complexAssociations,
        CancellationToken cancellationToken = default)
    {
        LastRoles = roles.ToList();
        LastComplexAssociations = complexAssociations.ToList();
        return Task.FromResult(new AuthToken("test-access-token", DateTimeOffset.UtcNow.AddMinutes(15)));
    }
}
