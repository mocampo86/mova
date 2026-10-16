using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
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
    private readonly IGetMyFavoriteComplexesHandler _myFavoritesHandler;
    private readonly IGetMyFavoriteComplexIdsHandler _myFavoriteIdsHandler;
    private readonly IAddFavoriteComplexHandler _addFavoriteHandler;
    private readonly IRemoveFavoriteComplexHandler _removeFavoriteHandler;
    private readonly IValidator<CompleteProfileCommand> _completeProfileValidator;
    private readonly IValidator<GetUserDashboardQuery> _dashboardValidator;
    private readonly IValidator<GetMyBlockStatusQuery> _myBlockStatusValidator;
    private readonly IValidator<GetMyFavoriteComplexesQuery> _myFavoritesValidator;
    private readonly IValidator<GetMyFavoriteComplexIdsQuery> _myFavoriteIdsValidator;
    private readonly IValidator<AddFavoriteComplexCommand> _addFavoriteValidator;
    private readonly IValidator<RemoveFavoriteComplexCommand> _removeFavoriteValidator;

    public UsersController(
        ICompleteProfileHandler completeProfileHandler,
        IGetUserDashboardHandler dashboardHandler,
        IGetMyBlockStatusHandler myBlockStatusHandler,
        IGetMyFavoriteComplexesHandler myFavoritesHandler,
        IGetMyFavoriteComplexIdsHandler myFavoriteIdsHandler,
        IAddFavoriteComplexHandler addFavoriteHandler,
        IRemoveFavoriteComplexHandler removeFavoriteHandler,
        IValidator<CompleteProfileCommand> completeProfileValidator,
        IValidator<GetUserDashboardQuery> dashboardValidator,
        IValidator<GetMyBlockStatusQuery> myBlockStatusValidator,
        IValidator<GetMyFavoriteComplexesQuery> myFavoritesValidator,
        IValidator<GetMyFavoriteComplexIdsQuery> myFavoriteIdsValidator,
        IValidator<AddFavoriteComplexCommand> addFavoriteValidator,
        IValidator<RemoveFavoriteComplexCommand> removeFavoriteValidator)
    {
        _completeProfileHandler = completeProfileHandler;
        _dashboardHandler = dashboardHandler;
        _myBlockStatusHandler = myBlockStatusHandler;
        _myFavoritesHandler = myFavoritesHandler;
        _myFavoriteIdsHandler = myFavoriteIdsHandler;
        _addFavoriteHandler = addFavoriteHandler;
        _removeFavoriteHandler = removeFavoriteHandler;
        _completeProfileValidator = completeProfileValidator;
        _dashboardValidator = dashboardValidator;
        _myBlockStatusValidator = myBlockStatusValidator;
        _myFavoritesValidator = myFavoritesValidator;
        _myFavoriteIdsValidator = myFavoriteIdsValidator;
        _addFavoriteValidator = addFavoriteValidator;
        _removeFavoriteValidator = removeFavoriteValidator;
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

    [HttpGet("me/favorites")]
    public async Task<ActionResult<PagedResult<FavoriteComplexInfo>>> GetMyFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetMyFavoriteComplexesQuery(userId, page, pageSize);
        await _myFavoritesValidator.ValidateAndThrowAsync(query, cancellationToken);

        var favorites = await _myFavoritesHandler.HandleAsync(query, cancellationToken);
        return Ok(favorites);
    }

    [HttpGet("me/favorites/ids")]
    public async Task<ActionResult<FavoriteComplexIdsInfo>> GetMyFavoriteIds(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetMyFavoriteComplexIdsQuery(userId);
        await _myFavoriteIdsValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await _myFavoriteIdsHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("me/favorites/{complexId:guid}")]
    public async Task<IActionResult> AddFavorite(
        Guid complexId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new AddFavoriteComplexCommand(userId, complexId);
        await _addFavoriteValidator.ValidateAndThrowAsync(command, cancellationToken);

        await _addFavoriteHandler.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("me/favorites/{complexId:guid}")]
    public async Task<IActionResult> RemoveFavorite(
        Guid complexId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new RemoveFavoriteComplexCommand(userId, complexId);
        await _removeFavoriteValidator.ValidateAndThrowAsync(command, cancellationToken);

        await _removeFavoriteHandler.HandleAsync(command, cancellationToken);
        return NoContent();
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
