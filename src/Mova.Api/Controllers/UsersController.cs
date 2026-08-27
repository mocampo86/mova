using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Contracts.Users;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = AuthorizationPolicies.User)]
public class UsersController : ControllerBase
{
    private readonly ICompleteProfileHandler _completeProfileHandler;
    private readonly IGetUserDashboardHandler _dashboardHandler;
    private readonly IGetMyBlockStatusHandler _myBlockStatusHandler;
    private readonly IValidator<CompleteProfileCommand> _completeProfileValidator;
    private readonly IValidator<GetUserDashboardQuery> _dashboardValidator;
    private readonly IValidator<GetMyBlockStatusQuery> _myBlockStatusValidator;

    public UsersController(
        ICompleteProfileHandler completeProfileHandler,
        IGetUserDashboardHandler dashboardHandler,
        IGetMyBlockStatusHandler myBlockStatusHandler,
        IValidator<CompleteProfileCommand> completeProfileValidator,
        IValidator<GetUserDashboardQuery> dashboardValidator,
        IValidator<GetMyBlockStatusQuery> myBlockStatusValidator)
    {
        _completeProfileHandler = completeProfileHandler;
        _dashboardHandler = dashboardHandler;
        _myBlockStatusHandler = myBlockStatusHandler;
        _completeProfileValidator = completeProfileValidator;
        _dashboardValidator = dashboardValidator;
        _myBlockStatusValidator = myBlockStatusValidator;
    }

    [HttpPatch("me")]
    public async Task<ActionResult<UserInfo>> CompleteProfile(
        [FromBody] CompleteProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CompleteProfileCommand(userId, request.PhoneNumber);
        await _completeProfileValidator.ValidateAndThrowAsync(command, cancellationToken);

        var userInfo = await _completeProfileHandler.HandleAsync(command, cancellationToken);
        return Ok(userInfo);
    }

    [HttpGet("me/dashboard")]
    public async Task<ActionResult<UserDashboardInfo>> GetMyDashboard(
        [FromQuery] int upcomingPage = 1,
        [FromQuery] int upcomingPageSize = 5,
        [FromQuery] int historyPageSize = 3,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetUserDashboardQuery(userId, upcomingPage, upcomingPageSize, historyPageSize);
        await _dashboardValidator.ValidateAndThrowAsync(query, cancellationToken);

        var dashboard = await _dashboardHandler.HandleAsync(query, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("me/blocks/{complexId:guid}")]
    public async Task<ActionResult<MyBlockStatusInfo>> GetMyBlockStatus(
        Guid complexId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetMyBlockStatusQuery(userId, complexId);
        await _myBlockStatusValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await _myBlockStatusHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
        {
            return Guid.Empty;
        }

        return userId;
    }
}
