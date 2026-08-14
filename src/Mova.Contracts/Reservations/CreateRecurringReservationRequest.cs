using System.Text.Json.Serialization;

namespace Mova.Contracts.Reservations;

public sealed class CreateRecurringReservationRequest
{
    [JsonPropertyName("courtId")]
    public Guid CourtId { get; set; }

    [JsonPropertyName("dayOfWeek")]
    public int DayOfWeek { get; set; }

    [JsonPropertyName("startTime")]
    public TimeOnly StartTime { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("startDate")]
    public DateOnly StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateOnly EndDate { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
