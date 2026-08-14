namespace Mova.Contracts.Reservations;

public sealed class UpdateCancellationPolicyRequest
{
    public int MinimumHours { get; set; }

    public bool AllowUserCancellation { get; set; }
}
