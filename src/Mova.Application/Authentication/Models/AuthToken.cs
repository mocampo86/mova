namespace Mova.Application.Authentication.Models;

public sealed record AuthToken(string AccessToken, DateTimeOffset ExpiresAt);
