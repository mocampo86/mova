using System.Text.Json.Serialization;

namespace Mova.Contracts.Users;

public sealed class CompleteProfileRequest
{
    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;
}
