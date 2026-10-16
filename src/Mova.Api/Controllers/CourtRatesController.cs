using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.CourtRates.Commands;
using Mova.Application.CourtRates.Handlers;
using Mova.Application.CourtRates.Queries;
using Mova.Contracts.CourtRates;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/courts/{courtId:guid}")]
public sealed class CourtRatesController(
    IGetCourtRatesHandler getRatesHandler,
    ICreateCourtRateHandler createHandler,
    IUpdateCourtRateHandler updateHandler,
    IUpdateCourtRateStatusHandler updateStatusHandler,
    IGetPriceQuoteHandler getPriceQuoteHandler,
    IValidator<CreateCourtRateCommand> createValidator,
    IValidator<UpdateCourtRateCommand> updateValidator,
    IValidator<UpdateCourtRateStatusCommand> updateStatusValidator,
    IValidator<GetCourtRatesQuery> getRatesValidator,
    IValidator<GetPriceQuoteQuery> getPriceQuoteValidator) : ControllerBase
{
    [HttpGet("rates")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public async Task<ActionResult<IReadOnlyCollection<CourtRateInfo>>> GetRates(
        Guid complexId, Guid courtId, CancellationToken cancellationToken)
    {
        var query = new GetCourtRatesQuery(complexId, courtId);
        await getRatesValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await getRatesHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("rates")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public async Task<ActionResult<CourtRateInfo>> Create(
        Guid complexId, Guid courtId, [FromBody] CreateCourtRateRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCourtRateCommand(
            complexId,
            courtId,
            request.DaysOfWeek.Select(d => (DayOfWeek)d).ToArray(),
            request.StartTime,
            request.EndTime,
            request.AmountPerHour);

        await createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/courts/{courtId}/rates/{result.Id}", result);
    }

    [HttpPut("rates/{rateId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public async Task<ActionResult<CourtRateInfo>> Update(
        Guid complexId, Guid courtId, Guid rateId, [FromBody] UpdateCourtRateRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCourtRateCommand(
            complexId,
            courtId,
            rateId,
            request.DaysOfWeek.Select(d => (DayOfWeek)d).ToArray(),
            request.StartTime,
            request.EndTime,
            request.AmountPerHour);

        await updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await updateHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("rates/{rateId:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public async Task<ActionResult<CourtRateInfo>> UpdateStatus(
        Guid complexId, Guid courtId, Guid rateId, [FromBody] UpdateCourtRateStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCourtRateStatusCommand(complexId, courtId, rateId, request.IsActive);
        await updateStatusValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await updateStatusHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("price-quote")]
    [AllowAnonymous]
    public async Task<ActionResult<PriceQuoteInfo>> GetPriceQuote(
        Guid complexId,
        Guid courtId,
        [FromQuery] DateTime startAt,
        [FromQuery] DateTime endAt,
        CancellationToken cancellationToken)
    {
        var query = new GetPriceQuoteQuery(complexId, courtId, startAt, endAt);
        await getPriceQuoteValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await getPriceQuoteHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }
}
