namespace Mova.Application.CourtRates.Commands;

public sealed record UpdateCourtRateStatusCommand(
    Guid SportsComplexId,
    Guid CourtId,
    Guid RateId,
    bool IsActive);
