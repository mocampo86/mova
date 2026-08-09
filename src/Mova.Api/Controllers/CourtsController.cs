using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Mova.Api.Authorization;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Courts;
using Mova.Domain.Enums;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/courts")]
[Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
public sealed class CourtsController(
    ICreateCourtHandler createHandler,
    IAssignCourtSportsHandler assignSportsHandler,
    IUpdateCourtStatusHandler updateStatusHandler,
    IUpdateCourtAvailabilityRulesHandler updateAvailabilityHandler,
    IGetCourtAvailabilityRulesHandler getAvailabilityHandler,
    IGetActiveCourtsByComplexHandler getActiveCourtsHandler,
    IGetCourtsByComplexHandler getCourtsHandler,
    IGetActiveCourtByIdHandler getActiveCourtByIdHandler,
    IAuthorizationService authorizationService,
    FluentValidation.IValidator<CreateCourtCommand> validator,
    FluentValidation.IValidator<AssignCourtSportsCommand> assignSportsValidator,
    FluentValidation.IValidator<UpdateCourtStatusCommand> updateStatusValidator,
    FluentValidation.IValidator<UpdateCourtAvailabilityRulesCommand> updateAvailabilityValidator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<CourtInfo>>> GetActiveList(
        Guid complexId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sportId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid pagination parameters." } });
        }

        var statusFilter = ParseStatusFilter(status);

        if (statusFilter != CourtStatus.Active)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(User, HttpContext, AuthorizationPolicies.ComplexAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        if (statusFilter == CourtStatus.Active)
        {
            var activeResult = await getActiveCourtsHandler.HandleAsync(
                new GetActiveCourtsByComplexQuery(complexId, page, pageSize, sportId),
                cancellationToken);

            return Ok(activeResult);
        }

        var courtStatus = statusFilter == CourtStatus.Inactive ? CourtStatus.Inactive : (CourtStatus?)null;

        var result = await getCourtsHandler.HandleAsync(
            new GetCourtsByComplexQuery(complexId, page, pageSize, sportId, courtStatus),
            cancellationToken);

        return Ok(result);
    }

    private static CourtStatus? ParseStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return CourtStatus.Active;
        }

        if (string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase))
        {
            return CourtStatus.Inactive;
        }

        if (string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return CourtStatus.Active;
    }

    [HttpGet("~/api/v1/courts/{courtId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CourtInfo>> GetActiveById(Guid courtId, CancellationToken cancellationToken)
    {
        var result = await getActiveCourtByIdHandler.HandleAsync(new GetActiveCourtByIdQuery(courtId), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CourtInfo>> Create(Guid complexId, CreateCourtRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCourtCommand(complexId, request.Name, request.Description, request.SurfaceType, request.Indoor, request.SportIds);
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/courts/{result.Id}", result);
    }

    [HttpPut("{courtId:guid}/sports")]
    public async Task<ActionResult<CourtInfo>> AssignSports(
        Guid complexId, Guid courtId, AssignCourtSportsRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignCourtSportsCommand(complexId, courtId, request.SportIds);
        await assignSportsValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await assignSportsHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{courtId:guid}/status")]
    public async Task<ActionResult<CourtInfo>> UpdateStatus(
        Guid complexId,
        Guid courtId,
        [FromBody] UpdateCourtStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<CourtStatus>(request.Status, out var status))
        {
            return BadRequest(new { error = new { message = "Status is not valid." } });
        }

        var command = new UpdateCourtStatusCommand(complexId, courtId, userId, status);

        await updateStatusValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await updateStatusHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{courtId:guid}/availability")]
    public async Task<ActionResult<IReadOnlyCollection<CourtAvailabilityRuleInfo>>> GetAvailability(
        Guid complexId, Guid courtId, CancellationToken cancellationToken)
    {
        var result = await getAvailabilityHandler.HandleAsync(new GetCourtAvailabilityRulesQuery(complexId, courtId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{courtId:guid}/availability")]
    public async Task<ActionResult<IReadOnlyCollection<CourtAvailabilityRuleInfo>>> UpdateAvailability(
        Guid complexId, Guid courtId, UpdateCourtAvailabilityRulesRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCourtAvailabilityRulesCommand(complexId, courtId, request.Rules
            .Select(r => new CourtAvailabilityRuleItem(r.DayOfWeek, r.StartTime, r.EndTime, r.SlotDurationMinutes, r.IsActive)).ToArray());
        await updateAvailabilityValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await updateAvailabilityHandler.HandleAsync(command, cancellationToken);
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
