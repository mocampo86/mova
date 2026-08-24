using System.Text.Json.Serialization;

namespace Mova.Contracts.Auth;

public sealed class CompleteComplexAdminRequest
{
    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("complexPhoneNumber")]
    public string ComplexPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("complexEmail")]
    public string ComplexEmail { get; set; } = string.Empty;

    [JsonPropertyName("timeZoneId")]
    public string TimeZoneId { get; set; } = string.Empty;
}
