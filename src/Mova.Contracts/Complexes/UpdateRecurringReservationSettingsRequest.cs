using System.Text.Json.Serialization;

namespace Mova.Contracts.Complexes;

public sealed class UpdateRecurringReservationSettingsRequest
{
    [JsonPropertyName("allowUserRecurringReservations")]
    public bool AllowUserRecurringReservations { get; set; }
}
