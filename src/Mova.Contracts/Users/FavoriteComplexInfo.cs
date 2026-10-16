using System.Text.Json.Serialization;

namespace Mova.Contracts.Users;

public sealed class FavoriteComplexInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("sports")]
    public IReadOnlyList<string> Sports { get; set; } = [];

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public sealed class FavoriteComplexIdsInfo
{
    [JsonPropertyName("items")]
    public IReadOnlyList<Guid> Items { get; set; } = [];
}
