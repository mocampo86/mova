using System.Text.Json.Serialization;

namespace Mova.Contracts.Complexes;

public sealed class UpdateComplexStatusRequest
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
