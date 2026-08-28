using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Mova.Api.HealthChecks;

namespace Mova.UnitTests.Api.Health;

public class ErrorRateHealthCheckTests
{
    private static ErrorRateHealthCheck CreateHealthCheck(IErrorRateTracker tracker, double maxErrorRatePerMinute)
    {
        var options = Options.Create(new ErrorRateHealthCheckOptions { MaxErrorRatePerMinute = maxErrorRatePerMinute });
        return new ErrorRateHealthCheck(tracker, options);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenErrorRateBelowThreshold_ReturnsHealthy()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
        var tracker = new ErrorRateTracker(timeProvider, Options.Create(new ErrorRateTrackerOptions()));
        var healthCheck = CreateHealthCheck(tracker, maxErrorRatePerMinute: 5.0);

        tracker.RecordServerError();

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("error-rate", healthCheck, HealthStatus.Unhealthy, ["readiness"])
        });

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenErrorRateExceedsThreshold_ReturnsUnhealthy()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
        var tracker = new ErrorRateTracker(timeProvider, Options.Create(new ErrorRateTrackerOptions()));
        var healthCheck = CreateHealthCheck(tracker, maxErrorRatePerMinute: 0.3);

        tracker.RecordServerError();
        tracker.RecordServerError();

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("error-rate", healthCheck, HealthStatus.Unhealthy, ["readiness"])
        });

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
