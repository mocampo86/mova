using System.Text.Json.Serialization;

namespace Mova.Contracts.Users;

public sealed class UserInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("phoneVerified")]
    public bool PhoneVerified { get; set; }
}
