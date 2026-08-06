using System.Text.Json.Serialization;

namespace Mova.Contracts.Courts;

public sealed class UpdateCourtStatusRequest
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
