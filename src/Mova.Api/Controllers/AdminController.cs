using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Application.Audit.Handlers;
using Mova.Application.Audit.Queries;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Contracts.Audit;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public class AdminController(
    IGetAllComplexesHandler getAllComplexesHandler,
    IGetAuditLogsHandler getAuditLogsHandler) : ControllerBase
{
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
        var result = await getAllComplexesHandler.HandleAsync(
            new GetAllComplexesQuery(page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("audit-logs")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    public async Task<ActionResult<PagedResult<AuditLogInfo>>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sportsComplexId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid pagination parameters." } });
        }

        var query = new GetAuditLogsQuery(
            sportsComplexId,
            userId,
            action,
            entityType,
            entityId,
            from,
            to);

        var result = await getAuditLogsHandler.HandleAsync(query, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("complexes/{complexId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
    public IActionResult ComplexAdminOnly(Guid complexId)
    {
        return Ok(new { message = "Complex admin access granted.", complexId });
    }
}
