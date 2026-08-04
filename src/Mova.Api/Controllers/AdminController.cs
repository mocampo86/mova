using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    [HttpGet("super")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    public IActionResult SuperAdminOnly()
    {
        return Ok(new { message = "Super admin access granted." });
    }

    [HttpGet("complexes/{complexId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public IActionResult ComplexAdminOnly(Guid complexId)
    {
        return Ok(new { message = "Complex admin access granted.", complexId });
    }
}
