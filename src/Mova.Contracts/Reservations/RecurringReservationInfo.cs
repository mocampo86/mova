using System.Text.Json.Serialization;

namespace Mova.Contracts.Reservations;

public sealed class RecurringReservationInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("complexId")]
    public Guid SportsComplexId { get; set; }

    [JsonPropertyName("courtId")]
    public Guid CourtId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

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

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("occurrences")]
    public IReadOnlyList<ReservationInfo> Occurrences { get; set; } = [];
}
