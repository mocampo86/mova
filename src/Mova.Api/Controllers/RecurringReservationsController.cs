using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/recurring-reservations")]
public sealed class RecurringReservationsController(
    ICreateRecurringReservationHandler createHandler,
    ICancelRecurringReservationHandler cancelHandler,
    IModifyRecurringReservationFutureHandler modifyFutureHandler,
    IGetRecurringReservationsByComplexHandler getListHandler,
    IValidator<CreateRecurringReservationCommand> createValidator,
    IValidator<CancelRecurringReservationCommand> cancelValidator,
    IValidator<ModifyRecurringReservationFutureCommand> modifyFutureValidator,
    IValidator<GetRecurringReservationsByComplexQuery> listValidator,
    IAuthorizationService authorizationService) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<RecurringReservationListItem>>> GetList(
        Guid complexId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? courtId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecurringReservationsByComplexQuery(complexId, page, pageSize, userId, courtId, status, sort);
        await listValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await getListHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("me")]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<RecurringReservationInfo>> CreateMyRecurringReservation(
        Guid complexId,
        CreateRecurringReservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (!Enum.IsDefined(typeof(DayOfWeek), request.DayOfWeek))
        {
            return BadRequest(new { error = new { message = "DayOfWeek is not valid." } });
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(
            User,
            HttpContext,
            AuthorizationPolicies.ComplexAdmin);

        var command = new CreateRecurringReservationCommand(
            complexId,
            request.CourtId,
            userId,
            (DayOfWeek)request.DayOfWeek,
            request.StartTime,
            request.DurationMinutes,
            request.StartDate,
            request.EndDate,
            request.Notes,
            authorizationResult.Succeeded,
            request.UtcOffsetMinutes);

        await createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/recurring-reservations/{result.Id}", result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public async Task<ActionResult<RecurringReservationInfo>> CreateRecurringReservationForCustomer(
        Guid complexId,
        [FromBody] CreateRecurringReservationForCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(DayOfWeek), request.DayOfWeek))
        {
            return BadRequest(new { error = new { message = "DayOfWeek is not valid." } });
        }

        var command = new CreateRecurringReservationCommand(
            complexId,
            request.CourtId,
            request.UserId,
            (DayOfWeek)request.DayOfWeek,
            request.StartTime,
            request.DurationMinutes,
            request.StartDate,
            request.EndDate,
            request.Notes,
            true,
            request.UtcOffsetMinutes);

        await createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/recurring-reservations/{result.Id}", result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<RecurringReservationInfo>> CancelRecurringReservation(
        Guid complexId,
        Guid id,
        [FromBody] CancelReservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(
            User,
            HttpContext,
            AuthorizationPolicies.ComplexAdmin);

        var command = new CancelRecurringReservationCommand(
            complexId,
            id,
            userId,
            request.Reason,
            authorizationResult.Succeeded);

        await cancelValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await cancelHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/future")]
    [Authorize(Policy = AuthorizationPolicies.User)]
    public async Task<ActionResult<RecurringReservationInfo>> ModifyFutureOccurrences(
        Guid complexId,
        Guid id,
        [FromBody] ModifyRecurringReservationFutureRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (!Enum.IsDefined(typeof(DayOfWeek), request.DayOfWeek))
        {
            return BadRequest(new { error = new { message = "DayOfWeek is not valid." } });
        }

        var command = new ModifyRecurringReservationFutureCommand(
            complexId,
            id,
            userId,
            request.EffectiveDate,
            (DayOfWeek)request.DayOfWeek,
            request.StartTime,
            request.DurationMinutes,
            request.EndDate,
            request.Notes,
            request.UtcOffsetMinutes);

        await modifyFutureValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await modifyFutureHandler.HandleAsync(command, cancellationToken);
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
