using System.Text.Json.Serialization;
using Mova.Contracts.Users;

namespace Mova.Contracts.Auth;

public sealed class CompleteComplexAdminResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public DateTimeOffset ExpiresAt { get; set; }

    [JsonPropertyName("user")]
    public UserInfo User { get; set; } = null!;

    [JsonPropertyName("complexId")]
    public Guid ComplexId { get; set; }

    [JsonPropertyName("requiresProfileCompletion")]
    public bool RequiresProfileCompletion { get; set; }
}
