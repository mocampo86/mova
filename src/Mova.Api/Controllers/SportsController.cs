using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Application.Courts.Handlers;
using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/sports")]
[Authorize]
public sealed class SportsController(IGetActiveSportsHandler handler) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<SportInfo>>> GetActive(CancellationToken cancellationToken = default)
    {
        var result = await handler.HandleAsync(new GetActiveSportsQuery(), cancellationToken);
        return Ok(result);
    }
}
