using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Mova.Api.HealthChecks;

public sealed class ErrorRateHealthCheck : IHealthCheck
{
    private readonly IErrorRateTracker _tracker;
    private readonly ErrorRateHealthCheckOptions _options;

    public ErrorRateHealthCheck(IErrorRateTracker tracker, IOptions<ErrorRateHealthCheckOptions> options)
    {
        _tracker = tracker;
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var snapshot = _tracker.GetSnapshot();

        if (snapshot.ErrorRatePerMinute > _options.MaxErrorRatePerMinute)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Error rate exceeded threshold: {snapshot.ErrorRatePerMinute:F2} errors/minute " +
                $"({snapshot.ErrorCount} server errors in the last {snapshot.Window.TotalMinutes:F0} minutes)."));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Error rate within threshold: {snapshot.ErrorRatePerMinute:F2} errors/minute " +
            $"({snapshot.ErrorCount} server errors in the last {snapshot.Window.TotalMinutes:F0} minutes)."));
    }
}

public sealed class ErrorRateHealthCheckOptions
{
    public const string SectionName = "ErrorRateHealthCheck";

    public double MaxErrorRatePerMinute { get; set; } = 5.0;
}
