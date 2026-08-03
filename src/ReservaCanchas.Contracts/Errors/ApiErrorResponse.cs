using System.Text.Json.Serialization;

namespace ReservaCanchas.Contracts.Errors;

public class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public ErrorDetails Error { get; set; } = null!;
}

public class ErrorDetails
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Details { get; set; }

    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }
}
