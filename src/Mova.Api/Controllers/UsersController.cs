using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Contracts.Users;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ICompleteProfileHandler _handler;
    private readonly IValidator<CompleteProfileCommand> _validator;

    public UsersController(ICompleteProfileHandler handler, IValidator<CompleteProfileCommand> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPatch("me")]
    public async Task<ActionResult<UserInfo>> CompleteProfile(
        [FromBody] CompleteProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var command = new CompleteProfileCommand(userId, request.PhoneNumber);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var userInfo = await _handler.HandleAsync(command, cancellationToken);
        return Ok(userInfo);
    }
}
