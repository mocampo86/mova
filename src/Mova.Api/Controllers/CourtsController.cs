using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Mova.Api.Authorization;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Handlers;
using Mova.Contracts.Courts;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/courts")]
[Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
public sealed class CourtsController(ICreateCourtHandler createHandler, FluentValidation.IValidator<CreateCourtCommand> validator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CourtInfo>> Create(Guid complexId, CreateCourtRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCourtCommand(complexId, request.Name, request.Description, request.SurfaceType, request.Indoor, request.SportIds);
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await createHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/courts/{result.Id}", result);
    }
}
