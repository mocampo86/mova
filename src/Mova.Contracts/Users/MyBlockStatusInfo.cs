using System.Text.Json.Serialization;

namespace Mova.Contracts.Users;

public sealed class MyBlockStatusInfo
{
    [JsonPropertyName("isBlocked")]
    public bool IsBlocked { get; set; }

    [JsonPropertyName("complexId")]
    public Guid ComplexId { get; set; }

    [JsonPropertyName("complexName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ComplexName { get; set; }

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }

    [JsonPropertyName("blockedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime BlockedAt { get; set; }

    [JsonPropertyName("blockedUntil")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? BlockedUntil { get; set; }
}
