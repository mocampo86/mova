namespace Mova.Contracts.Courts;

public sealed class CourtInfo
{
    public Guid Id { get; set; }
    public Guid SportsComplexId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SurfaceType { get; set; } = string.Empty;
    public bool Indoor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyCollection<Guid> SportIds { get; set; } = [];
}
