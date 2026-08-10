using Mova.Domain.Enums;

namespace Mova.Application.Courts.Queries;

public sealed record GetCourtsByComplexQuery(Guid SportsComplexId, int Page = 1, int PageSize = 20, Guid? SportId = null, CourtStatus? Status = null);
