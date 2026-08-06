using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Queries;
using Mova.Application.Courts.Validators;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;
using Mova.Domain.Enums;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes")]
[Authorize]
public class ComplexesController : ControllerBase
{
    private readonly ICreateComplexHandler _createHandler;
    private readonly IUpdateComplexHandler _updateHandler;
    private readonly IGetActiveComplexesHandler _getActiveComplexesHandler;
    private readonly IGetActiveComplexByIdHandler _getActiveComplexByIdHandler;
    private readonly IUpdateComplexStatusHandler _updateComplexStatusHandler;
    private readonly IGetCourtAvailabilityHandler _getCourtAvailabilityHandler;
    private readonly IValidator<CreateComplexCommand> _createValidator;
    private readonly IValidator<UpdateComplexCommand> _updateValidator;
    private readonly IValidator<UpdateComplexStatusCommand> _updateComplexStatusValidator;
    private readonly IValidator<GetCourtAvailabilityQuery> _getCourtAvailabilityValidator;

    public ComplexesController(
        ICreateComplexHandler createHandler,
        IUpdateComplexHandler updateHandler,
        IGetActiveComplexesHandler getActiveComplexesHandler,
        IGetActiveComplexByIdHandler getActiveComplexByIdHandler,
        IUpdateComplexStatusHandler updateComplexStatusHandler,
        IGetCourtAvailabilityHandler getCourtAvailabilityHandler,
        IValidator<CreateComplexCommand> createValidator,
        IValidator<UpdateComplexCommand> updateValidator,
        IValidator<UpdateComplexStatusCommand> updateComplexStatusValidator,
        IValidator<GetCourtAvailabilityQuery> getCourtAvailabilityValidator)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _getActiveComplexesHandler = getActiveComplexesHandler;
        _getActiveComplexByIdHandler = getActiveComplexByIdHandler;
        _updateComplexStatusHandler = updateComplexStatusHandler;
        _getCourtAvailabilityHandler = getCourtAvailabilityHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updateComplexStatusValidator = updateComplexStatusValidator;
        _getCourtAvailabilityValidator = getCourtAvailabilityValidator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<SportsComplexInfo>>> GetActiveList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100 || search?.Length > 100)
        {
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid pagination or search parameters." } });
        }

        var result = await _getActiveComplexesHandler.HandleAsync(
            new GetActiveComplexesQuery(page, pageSize, search),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{complexId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<SportsComplexInfo>> GetActiveById(
        Guid complexId,
        CancellationToken cancellationToken = default)
    {
        var result = await _getActiveComplexByIdHandler.HandleAsync(
            new GetActiveComplexByIdQuery(complexId),
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{complexId:guid}/availability")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<CourtAvailabilitySlotInfo>>> GetAvailability(
        Guid complexId,
        [FromQuery] Guid courtId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCourtAvailabilityQuery(complexId, courtId, date);
        await _getCourtAvailabilityValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await _getCourtAvailabilityHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<SportsComplexInfo>> Create(
        [FromBody] CreateComplexRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CreateComplexCommand(
            userId,
            request.Name,
            request.Description,
            request.Address,
            request.City,
            request.Latitude,
            request.Longitude,
            request.PhoneNumber,
            request.Email);

        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await _createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{result.Id}", result);
    }

    [HttpPut("{complexId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public async Task<ActionResult<SportsComplexInfo>> Update(
        Guid complexId,
        [FromBody] UpdateComplexRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<ComplexStatus>(request.Status, out var status))
        {
            return BadRequest(new { error = new { message = "Status is not valid." } });
        }

        var command = new UpdateComplexCommand(
            complexId,
            userId,
            request.Name,
            request.Description,
            request.Address,
            request.City,
            request.Latitude,
            request.Longitude,
            request.PhoneNumber,
            request.Email,
            status);

        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await _updateHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{complexId:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public async Task<ActionResult<SportsComplexInfo>> UpdateStatus(
        Guid complexId,
        [FromBody] UpdateComplexStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<ComplexStatus>(request.Status, out var status))
        {
            return BadRequest(new { error = new { message = "Status is not valid." } });
        }

        var command = new UpdateComplexStatusCommand(complexId, userId, status);

        await _updateComplexStatusValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await _updateComplexStatusHandler.HandleAsync(command, cancellationToken);
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
