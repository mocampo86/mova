using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/business-hours")]
[Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
public sealed class BusinessHoursController(
    IUpdateBusinessHoursHandler updateHandler,
    IGetBusinessHoursHandler getHandler,
    IValidator<UpdateBusinessHoursCommand> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<BusinessHoursInfo>>> Get(Guid complexId, CancellationToken cancellationToken)
    {
        var result = await getHandler.HandleAsync(new GetBusinessHoursQuery(complexId), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<IReadOnlyCollection<BusinessHoursInfo>>> Update(
        Guid complexId, UpdateBusinessHoursRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBusinessHoursCommand(complexId, request.Hours
            .Select(h => new BusinessHoursItem(h.DayOfWeek, h.OpeningTime, h.ClosingTime, h.IsClosed)).ToArray());
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await updateHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }
}
