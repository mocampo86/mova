using System.Text.Json.Serialization;

namespace Mova.Contracts.Reservations;

public sealed class ReservationPriceLineInfo
{
    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    [JsonPropertyName("hourlyRate")]
    public decimal HourlyRate { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}
