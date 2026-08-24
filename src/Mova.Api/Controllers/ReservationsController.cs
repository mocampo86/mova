using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Mova.Api.Authorization;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;
using Mova.Domain.Enums;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/reservations")]
public sealed class ReservationsController(
    ICreateReservationHandler createHandler,
    IGetReservationsByComplexHandler getListHandler,
    IGetMyUpcomingReservationsHandler getMyUpcomingHandler,
    IGetMyReservationHistoryHandler getMyHistoryHandler,
    IGetReservationByIdHandler getByIdHandler,
    ICancelReservationHandler cancelHandler,
    ICancelMyReservationHandler cancelMyHandler,
    IUpdateReservationStatusHandler updateStatusHandler,
    FluentValidation.IValidator<CreateReservationCommand> createValidator,
    FluentValidation.IValidator<GetReservationsByComplexQuery> listValidator,
    FluentValidation.IValidator<GetMyUpcomingReservationsQuery> myUpcomingValidator,
    FluentValidation.IValidator<GetMyReservationHistoryQuery> myHistoryValidator,
    FluentValidation.IValidator<CancelReservationCommand> cancelValidator,
    FluentValidation.IValidator<CancelMyReservationCommand> cancelMyValidator,
    FluentValidation.IValidator<UpdateReservationStatusCommand> updateStatusValidator) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReservationInfo>>> GetList(
        Guid complexId,
        [FromQuery] Guid? courtId,
        [FromQuery] string? status,
        [FromQuery] DateOnly? date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReservationsByComplexQuery(complexId, page, pageSize, courtId, status, date, sort, userId);
        await listValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await getListHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("~/api/v1/users/me/reservations")]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<PagedResult<ReservationInfo>>> GetMyUpcoming(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetMyUpcomingReservationsQuery(userId, DateTime.UtcNow, page, pageSize);
        await myUpcomingValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await getMyUpcomingHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    [HttpPost]
    public async Task<ActionResult<ReservationInfo>> Create(Guid complexId, CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var createdByUserId = GetCurrentUserId();
        if (createdByUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CreateReservationCommand(
            complexId,
            request.CourtId,
            request.UserId,
            createdByUserId,
            request.StartAt,
            request.EndAt,
            request.Notes);

        await createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/reservations/{result.Id}", result);
    }

    [HttpGet("~/api/v1/users/me/reservations/history")]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<PagedResult<ReservationInfo>>> GetMyHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetMyReservationHistoryQuery(userId, page, pageSize);
        await myHistoryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await getMyHistoryHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("~/api/v1/users/me/reservations/{id:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<ReservationInfo>> CancelMyReservation(Guid id, [FromBody] CancelReservationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CancelMyReservationCommand(id, userId, request.Reason);
        await cancelMyValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await cancelMyHandler.HandleAsync(command, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("me")]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<ReservationInfo>> CreateMyReservation(Guid complexId, CreateMyReservationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CreateReservationCommand(
            complexId,
            request.CourtId,
            userId,
            userId,
            request.StartAt,
            request.EndAt,
            request.Notes,
            ReservationSource.Web);

        await createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/reservations/{result.Id}", result);
    }

    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationInfo>> GetById(Guid complexId, Guid id, CancellationToken cancellationToken)
    {
        var result = await getByIdHandler.HandleAsync(new GetReservationByIdQuery(complexId, id), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationInfo>> Cancel(Guid complexId, Guid id, [FromBody] CancelReservationRequest request, CancellationToken cancellationToken)
    {
        var cancelledByUserId = GetCurrentUserId();
        if (cancelledByUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CancelReservationCommand(complexId, id, cancelledByUserId, request.Reason);
        await cancelValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await cancelHandler.HandleAsync(command, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ReservationInfo>> UpdateStatus(
        Guid complexId,
        Guid id,
        [FromBody] UpdateReservationStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ReservationStatus>(request.Status, out var status))
        {
            return BadRequest(new { error = new { message = "Status is not valid." } });
        }

        var command = new UpdateReservationStatusCommand(complexId, id, status);
        await updateStatusValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await updateStatusHandler.HandleAsync(command, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

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
