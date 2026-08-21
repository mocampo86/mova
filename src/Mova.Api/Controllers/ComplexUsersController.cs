using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;
using Mova.Contracts.Users;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/users")]
[Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
public sealed class ComplexUsersController : ControllerBase
{
    private readonly IGetUsersByComplexHandler _getUsersHandler;
    private readonly ISearchUsersHandler _searchUsersHandler;
    private readonly IGetReservationsByComplexHandler _getReservationsHandler;
    private readonly IValidator<GetUsersByComplexQuery> _listValidator;
    private readonly IValidator<SearchUsersQuery> _searchValidator;
    private readonly IValidator<GetReservationsByComplexQuery> _reservationsValidator;

    public ComplexUsersController(
        IGetUsersByComplexHandler getUsersHandler,
        ISearchUsersHandler searchUsersHandler,
        IGetReservationsByComplexHandler getReservationsHandler,
        IValidator<GetUsersByComplexQuery> listValidator,
        IValidator<SearchUsersQuery> searchValidator,
        IValidator<GetReservationsByComplexQuery> reservationsValidator)
    {
        _getUsersHandler = getUsersHandler;
        _searchUsersHandler = searchUsersHandler;
        _getReservationsHandler = getReservationsHandler;
        _listValidator = listValidator;
        _searchValidator = searchValidator;
        _reservationsValidator = reservationsValidator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ComplexUserInfo>>> GetList(
        Guid complexId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersByComplexQuery(complexId, page, pageSize, search, sort);
        await _listValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await _getUsersHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ComplexUserInfo>>> Search(
        Guid complexId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchUsersQuery(complexId, search, page, pageSize, sort);
        await _searchValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await _searchUsersHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:guid}/reservations")]
    public async Task<ActionResult<PagedResult<ReservationInfo>>> GetUserReservations(
        Guid complexId,
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = "startAt:desc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetReservationsByComplexQuery(complexId, page, pageSize, null, null, null, sort, userId);
        await _reservationsValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await _getReservationsHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }
}
