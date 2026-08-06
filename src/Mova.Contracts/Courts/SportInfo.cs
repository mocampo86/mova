using System.Text.Json.Serialization;

namespace Mova.Contracts.Courts;

public sealed class SportInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
