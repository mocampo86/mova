using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Api.Authorization;
using Mova.Api.Filters;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Contracts.Users;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/complexes/{complexId:guid}/blocked-users")]
[Authorize(Policy = AuthorizationPolicies.ComplexAdmin)]
public sealed class BlockedUsersController : ControllerBase
{
    private readonly IBlockUserHandler _blockHandler;
    private readonly IUnblockUserHandler _unblockHandler;
    private readonly IValidator<BlockUserCommand> _blockValidator;
    private readonly IValidator<UnblockUserCommand> _unblockValidator;

    public BlockedUsersController(
        IBlockUserHandler blockHandler,
        IUnblockUserHandler unblockHandler,
        IValidator<BlockUserCommand> blockValidator,
        IValidator<UnblockUserCommand> unblockValidator)
    {
        _blockHandler = blockHandler;
        _unblockHandler = unblockHandler;
        _blockValidator = blockValidator;
        _unblockValidator = unblockValidator;
    }

    [HttpPost]
    [IdempotencyRequired]
    public async Task<ActionResult<BlockedUserInfo>> Block(
        Guid complexId,
        [FromBody] BlockUserRequest request,
        CancellationToken cancellationToken)
    {
        var blockedByUserId = GetCurrentUserId();
        if (blockedByUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new BlockUserCommand(
            complexId,
            request.UserId,
            blockedByUserId,
            request.Reason,
            request.BlockedUntil);

        await _blockValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await _blockHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/complexes/{complexId}/blocked-users/{result.Id}", result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<BlockedUserInfo>> Unblock(Guid complexId, Guid id, CancellationToken cancellationToken)
    {
        var command = new UnblockUserCommand(complexId, id);
        await _unblockValidator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await _unblockHandler.HandleAsync(command, cancellationToken);
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
