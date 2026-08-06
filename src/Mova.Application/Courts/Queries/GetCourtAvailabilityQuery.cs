namespace Mova.Application.Courts.Queries;

public sealed record GetCourtAvailabilityQuery(Guid SportsComplexId, Guid CourtId, DateOnly Date);
