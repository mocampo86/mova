namespace Mova.Application.Courts.Commands;

public sealed record AssignCourtSportsCommand(Guid SportsComplexId, Guid CourtId, IReadOnlyCollection<Guid> SportIds);
