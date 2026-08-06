namespace Mova.Application.Courts.Commands;

public sealed record CreateCourtCommand(Guid SportsComplexId, string Name, string Description, string SurfaceType, bool Indoor, IReadOnlyCollection<Guid>? SportIds);
