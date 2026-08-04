using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Enums;

namespace Mova.Api.Authorization;

public sealed class ComplexAdminAuthorizationHandler : AuthorizationHandler<ComplexAdminRequirement>
{
    private readonly IComplexAdministratorRepository _complexAdministratorRepository;

    public ComplexAdminAuthorizationHandler(IComplexAdministratorRepository complexAdministratorRepository)
    {
        _complexAdministratorRepository = complexAdministratorRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ComplexAdminRequirement requirement)
    {
        if (context.User.IsInRole(AuthorizationPolicies.SuperAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (!context.User.IsInRole(AuthorizationPolicies.ComplexAdmin))
        {
            return;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return;
        }

        var userIdValue = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;

        var complexIdValue = httpContext.Request.RouteValues["complexId"] as string
            ?? httpContext.Request.Query["complexId"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userIdValue)
            || !Guid.TryParse(userIdValue, out var userId)
            || string.IsNullOrWhiteSpace(complexIdValue)
            || !Guid.TryParse(complexIdValue, out var requestedComplexId))
        {
            return;
        }

        var association = await _complexAdministratorRepository.GetByUserAndComplexAsync(
            userId,
            requestedComplexId,
            httpContext.RequestAborted);

        if (association is not null && association.Role == Role.ComplexAdmin)
        {
            context.Succeed(requirement);
        }
    }
}
