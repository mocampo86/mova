using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IGetAllComplexesHandler _getAllComplexesHandler;

    public AdminController(IGetAllComplexesHandler getAllComplexesHandler)
    {
        _getAllComplexesHandler = getAllComplexesHandler;
    }

    [HttpGet("super")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    public IActionResult SuperAdminOnly()
    {
        return Ok(new { message = "Super admin access granted." });
    }

    [HttpGet("complexes")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    public async Task<ActionResult<PagedResult<SportsComplexInfo>>> GetAllComplexes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _getAllComplexesHandler.HandleAsync(
            new GetAllComplexesQuery(page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("complexes/{complexId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public IActionResult ComplexAdminOnly(Guid complexId)
    {
        return Ok(new { message = "Complex admin access granted.", complexId });
    }
}
