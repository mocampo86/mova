namespace Mova.Infrastructure.Configuration;

public sealed class CancellationPolicyOptions
{
    public const string SectionName = "CancellationPolicy";

    public int MinimumHours { get; set; } = 24;

    public bool AllowUserCancellation { get; set; } = true;
}
