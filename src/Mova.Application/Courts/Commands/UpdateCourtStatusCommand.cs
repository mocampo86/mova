using Mova.Domain.Enums;

namespace Mova.Application.Courts.Commands;

public sealed record UpdateCourtStatusCommand(
    Guid SportsComplexId,
    Guid CourtId,
    Guid UserId,
    CourtStatus Status);
