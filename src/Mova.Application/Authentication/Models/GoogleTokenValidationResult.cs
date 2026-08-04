namespace Mova.Application.Authentication.Models;

public sealed record GoogleTokenValidationResult(
    string Subject,
    string Email,
    string Name);
