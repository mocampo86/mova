using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Mova.Api.Authorization;

public sealed class ComplexAdminAuthorizationHandler : AuthorizationHandler<ComplexAdminRequirement>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ComplexAdminRequirement requirement)
    {
        if (context.User.IsInRole(AuthorizationPolicies.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!context.User.IsInRole(AuthorizationPolicies.ComplexAdmin))
        {
            return Task.CompletedTask;
        }

        var complexId = GetRequestedComplexId(context);
        if (complexId is null || !Guid.TryParse(complexId, out var requestedComplexId))
        {
            return Task.CompletedTask;
        }

        var complexesClaim = context.User.FindFirst("complexes")?.Value;
        if (string.IsNullOrWhiteSpace(complexesClaim))
        {
            return Task.CompletedTask;
        }

        var associations = JsonSerializer.Deserialize<List<ComplexAssociation>>(complexesClaim, JsonOptions);
        if (associations is null)
        {
            return Task.CompletedTask;
        }

        if (associations.Any(a => a.ComplexId == requestedComplexId && a.Role == AuthorizationPolicies.ComplexAdmin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static string? GetRequestedComplexId(AuthorizationHandlerContext context)
    {
        if (context.Resource is HttpContext httpContext)
        {
            return httpContext.Request.RouteValues["complexId"] as string
                ?? httpContext.Request.Query["complexId"].FirstOrDefault();
        }

        return null;
    }

    private sealed record ComplexAssociation(Guid ComplexId, string Role);
}
