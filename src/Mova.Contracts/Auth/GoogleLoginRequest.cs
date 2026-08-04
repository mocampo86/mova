using System.Text.Json.Serialization;

namespace Mova.Contracts.Auth;

public sealed class GoogleLoginRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
}
