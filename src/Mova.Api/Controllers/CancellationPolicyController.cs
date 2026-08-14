using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Contracts.Reservations;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/configuration/cancellation-policy")]
[Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
public sealed class CancellationPolicyController(
    IGetCancellationPolicyHandler getHandler,
    IUpdateCancellationPolicyHandler updateHandler,
    IValidator<GetCancellationPolicyQuery> getValidator,
    IValidator<UpdateCancellationPolicyCommand> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CancellationPolicyInfo>> Get(Guid complexId, CancellationToken cancellationToken)
    {
        var query = new GetCancellationPolicyQuery(complexId);
        await getValidator.ValidateAndThrowAsync(query, cancellationToken);

        var result = await getHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<CancellationPolicyInfo>> Update(
        Guid complexId,
        UpdateCancellationPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCancellationPolicyCommand(complexId, request.MinimumHours, request.AllowUserCancellation);
        await updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await updateHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }
}
