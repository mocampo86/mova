using System.Text.Json.Serialization;

namespace Mova.Contracts.Courts;

public sealed class CourtAvailabilitySlotInfo
{
    [JsonPropertyName("courtId")]
    public Guid CourtId { get; set; }

    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }
}
