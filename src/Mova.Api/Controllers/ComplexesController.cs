using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Handlers;
using Mova.Contracts.Complexes;
using Mova.Domain.Enums;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes")]
[Authorize]
public class ComplexesController : ControllerBase
{
    private readonly ICreateComplexHandler _createHandler;
    private readonly IUpdateComplexHandler _updateHandler;
    private readonly IValidator<CreateComplexCommand> _createValidator;
    private readonly IValidator<UpdateComplexCommand> _updateValidator;

    public ComplexesController(
        ICreateComplexHandler createHandler,
        IUpdateComplexHandler updateHandler,
        IValidator<CreateComplexCommand> createValidator,
        IValidator<UpdateComplexCommand> updateValidator)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
