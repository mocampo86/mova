namespace Mova.Infrastructure.Authentication.Options;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "Google";

    public string ClientId { get; set; } = string.Empty;
}
