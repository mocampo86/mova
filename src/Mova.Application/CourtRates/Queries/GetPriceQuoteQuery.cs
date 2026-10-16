namespace Mova.Application.CourtRates.Queries;

public sealed record GetPriceQuoteQuery(
    Guid SportsComplexId,
    Guid CourtId,
    DateTime StartAt,
    DateTime EndAt);
