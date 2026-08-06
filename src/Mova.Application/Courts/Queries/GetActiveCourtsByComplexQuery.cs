namespace Mova.Application.Courts.Queries;

public sealed record GetActiveCourtsByComplexQuery(Guid SportsComplexId, int Page = 1, int PageSize = 20, Guid? SportId = null);
