namespace Mova.Infrastructure.Authentication.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Mova";
    public string Audience { get; set; } = "Mova.Api";
    public int ExpirationMinutes { get; set; } = 15;
}
