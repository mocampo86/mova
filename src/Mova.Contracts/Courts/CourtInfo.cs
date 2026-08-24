using System.Text.Json.Serialization;

namespace Mova.Contracts.Courts;

public sealed class CourtInfo
{
    public Guid Id { get; set; }
    public Guid SportsComplexId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SurfaceType { get; set; } = string.Empty;
    public bool Indoor { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyCollection<Guid> SportIds { get; set; } = [];
}
