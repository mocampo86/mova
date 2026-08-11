using System.Text.Json.Serialization;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;

namespace Mova.Contracts.Users;

public sealed class UserDashboardInfo
{
    [JsonPropertyName("user")]
    public UserInfo User { get; set; } = new();

    [JsonPropertyName("upcomingReservations")]
    public PagedResult<ReservationInfo> UpcomingReservations { get; set; } = new();

    [JsonPropertyName("historySummary")]
    public ReservationHistorySummaryInfo HistorySummary { get; set; } = new();

    [JsonPropertyName("activeBlocks")]
    public IReadOnlyList<UserBlockInfo> ActiveBlocks { get; set; } = [];
}

public sealed class ReservationHistorySummaryInfo
{
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("recentReservations")]
    public IReadOnlyList<ReservationInfo> RecentReservations { get; set; } = [];
}

public sealed class UserBlockInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("complexId")]
    public Guid ComplexId { get; set; }

    [JsonPropertyName("complexName")]
    public string ComplexName { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }

    [JsonPropertyName("blockedAt")]
    public DateTime BlockedAt { get; set; }

    [JsonPropertyName("blockedUntil")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? BlockedUntil { get; set; }
}
