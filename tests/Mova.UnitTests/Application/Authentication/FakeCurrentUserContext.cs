using Mova.Application.Abstractions.Authentication;

namespace Mova.UnitTests.Application.Authentication;

public sealed class FakeCurrentUserContext : ICurrentUserContext
{
    public Guid? UserId { get; set; }
    public bool IsAuthenticated { get; set; } = true;
    public List<string> Roles { get; } = [];

    IReadOnlyCollection<string> ICurrentUserContext.Roles => Roles;
}
