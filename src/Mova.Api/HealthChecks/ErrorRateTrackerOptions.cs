namespace Mova.Api.HealthChecks;

public sealed class ErrorRateTrackerOptions
{
    public const string SectionName = "ErrorRateTracker";

    public TimeSpan EvaluationWindow { get; set; } = TimeSpan.FromMinutes(5);

    public int MaxQueueSize { get; set; } = 1000;
}
