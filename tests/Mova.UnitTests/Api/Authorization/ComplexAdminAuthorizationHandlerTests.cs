using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Mova.Api.Authorization;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Api.Authorization;

public class ComplexAdminAuthorizationHandlerTests
{
    private readonly FakeComplexAdministratorRepository _repository = new();

    private ComplexAdminAuthorizationHandler CreateHandler() =>
        new(_repository);

    private static ClaimsPrincipal CreateUser(Guid userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new("sub", userId.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test", "sub", "roles"));
    }

    private static AuthorizationHandlerContext CreateContext(
        ClaimsPrincipal user,
        Guid complexId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["complexId"] = complexId.ToString();

        return new AuthorizationHandlerContext(
            [new ComplexAdminRequirement()],
            user,
            httpContext);
    }

    [Fact]
    public async Task HandleAsync_WithActiveComplexAdministratorAssociation_Succeeds()
    {
        var userId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var association = ComplexAdministrator.Create(complexId, userId, Role.ComplexAdmin);
        await _repository.AddAsync(association);

        var handler = CreateHandler();
        var context = CreateContext(CreateUser(userId, Role.User.ToString()), complexId);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WithSuperAdminRole_Succeeds()
    {
        var userId = Guid.NewGuid();
        var complexId = Guid.NewGuid();

        var handler = CreateHandler();
        var context = CreateContext(CreateUser(userId, Role.SuperAdmin.ToString()), complexId);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WithAssociationForDifferentComplex_DoesNotSucceed()
    {
        var userId = Guid.NewGuid();
        var ownedComplexId = Guid.NewGuid();
        var requestedComplexId = Guid.NewGuid();
        var association = ComplexAdministrator.Create(ownedComplexId, userId, Role.ComplexAdmin);
        await _repository.AddAsync(association);

        var handler = CreateHandler();
        var context = CreateContext(CreateUser(userId, Role.ComplexAdmin.ToString()), requestedComplexId);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveAssociation_DoesNotSucceed()
    {
        var userId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var association = ComplexAdministrator.Create(complexId, userId, Role.ComplexAdmin);
        association.Deactivate();
        await _repository.AddAsync(association);

        var handler = CreateHandler();
        var context = CreateContext(CreateUser(userId, Role.ComplexAdmin.ToString()), complexId);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WithoutAssociationOrSuperAdminRole_DoesNotSucceed()
    {
        var userId = Guid.NewGuid();
        var complexId = Guid.NewGuid();

        var handler = CreateHandler();
        var context = CreateContext(CreateUser(userId, Role.User.ToString()), complexId);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidUserId_DoesNotSucceed()
    {
        var complexId = Guid.NewGuid();
        var claims = new List<Claim> { new("sub", "not-a-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var handler = CreateHandler();
        var context = CreateContext(user, complexId);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
