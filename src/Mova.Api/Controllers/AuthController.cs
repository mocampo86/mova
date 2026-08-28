using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mova.Application.Authentication.Commands;
using Mova.Application.Authentication.Handlers;
using Mova.Contracts.Auth;

namespace Mova.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IGoogleLoginHandler _googleLoginHandler;
    private readonly ICompleteComplexAdminHandler _completeComplexAdminHandler;
    private readonly IValidator<GoogleLoginCommand> _googleLoginValidator;
    private readonly IValidator<CompleteComplexAdminCommand> _completeComplexAdminValidator;

    public AuthController(
        IGoogleLoginHandler googleLoginHandler,
        ICompleteComplexAdminHandler completeComplexAdminHandler,
        IValidator<GoogleLoginCommand> googleLoginValidator,
        IValidator<CompleteComplexAdminCommand> completeComplexAdminValidator)
    {
        _googleLoginHandler = googleLoginHandler;
        _completeComplexAdminHandler = completeComplexAdminHandler;
        _googleLoginValidator = googleLoginValidator;
        _completeComplexAdminValidator = completeComplexAdminValidator;
    }

    [HttpPost("google")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<GoogleLoginResponse>> GoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GoogleLoginCommand(request.IdToken);
        await _googleLoginValidator.ValidateAndThrowAsync(command, cancellationToken);

        var response = await _googleLoginHandler.HandleAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("complete-complex-admin")]
    [Authorize]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<CompleteComplexAdminResponse>> CompleteComplexAdmin(
        [FromBody] CompleteComplexAdminRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CompleteComplexAdminCommand(
            userId,
            request.PhoneNumber,
            request.Name,
            request.Description,
            request.Address,
            request.City,
            request.Latitude,
            request.Longitude,
            request.ComplexPhoneNumber,
            request.ComplexEmail,
            request.TimeZoneId);

        await _completeComplexAdminValidator.ValidateAndThrowAsync(command, cancellationToken);

        var response = await _completeComplexAdminHandler.HandleAsync(command, cancellationToken);
        return Ok(response);
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
