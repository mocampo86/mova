namespace Mova.Application.Reservations.Commands;

public sealed record UpdateCancellationPolicyCommand(
    Guid SportsComplexId,
    int MinimumHours,
    bool AllowUserCancellation);
