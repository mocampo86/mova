namespace Mova.Application.Courts.Commands;

public sealed record UpdateCourtCommand(
    Guid SportsComplexId,
    Guid CourtId,
    string Name,
    string Description,
    string SurfaceType,
    bool Indoor,
    IReadOnlyCollection<Guid>? SportIds);
