using System.Text.Json.Serialization;

namespace Mova.Contracts.Users;

public sealed class BlockedUserInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("sportsComplexId")]
    public Guid SportsComplexId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }

    [JsonPropertyName("blockedAt")]
    public DateTime BlockedAt { get; set; }

    [JsonPropertyName("blockedUntil")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? BlockedUntil { get; set; }

    [JsonPropertyName("blockedByUserId")]
    public Guid BlockedByUserId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
