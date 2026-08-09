namespace Mova.Contracts.Courts;

public sealed class UpdateCourtRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SurfaceType { get; set; } = string.Empty;
    public bool Indoor { get; set; }
    public IReadOnlyCollection<Guid>? SportIds { get; set; }
}
