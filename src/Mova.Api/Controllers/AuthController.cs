using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mova.Application.Authentication.Commands;
using Mova.Application.Authentication.Handlers;
using Mova.Contracts.Auth;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IGoogleLoginHandler _handler;
    private readonly IValidator<GoogleLoginCommand> _validator;

    public AuthController(IGoogleLoginHandler handler, IValidator<GoogleLoginCommand> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost("google")]
    public async Task<ActionResult<GoogleLoginResponse>> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GoogleLoginCommand(request.IdToken);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var response = await _handler.HandleAsync(command, cancellationToken);
        return Ok(response);
    }
}
