namespace Mova.Contracts.Reservations;

public sealed class CancellationPolicyInfo
{
    public Guid SportsComplexId { get; set; }

    public int MinimumHours { get; set; }

    public bool AllowUserCancellation { get; set; }
}
