using System.Text.Json.Serialization;

namespace Mova.Contracts.Complexes;

public sealed class ComplexDashboardInfo
{
    [JsonPropertyName("complex")]
    public DashboardComplexSummary Complex { get; set; } = new();

    [JsonPropertyName("courts")]
    public DashboardCourtSummary Courts { get; set; } = new();

    [JsonPropertyName("reservationsToday")]
    public DashboardReservationsSummary ReservationsToday { get; set; } = new();

    [JsonPropertyName("blockedUsers")]
    public int BlockedUsers { get; set; }
}

public sealed class DashboardComplexSummary
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("lastUpdatedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? LastUpdatedAt { get; set; }
}

public sealed class DashboardCourtSummary
{
    [JsonPropertyName("active")]
    public int Active { get; set; }

    [JsonPropertyName("inactive")]
    public int Inactive { get; set; }
}

public sealed class DashboardReservationsSummary
{
    [JsonPropertyName("confirmed")]
    public int Confirmed { get; set; }

    [JsonPropertyName("cancelled")]
    public int Cancelled { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }
}
