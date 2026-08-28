using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Mova.Api.HealthChecks;

namespace Mova.UnitTests.Api.Health;

public class ErrorRateHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenErrorCountBelowThreshold_ReturnsHealthy()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
        var tracker = new ErrorRateTracker(timeProvider);
        var options = Options.Create(new ErrorRateHealthCheckOptions { MaxErrorCount = 5 });
        var healthCheck = new ErrorRateHealthCheck(tracker, options);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("error-rate", healthCheck, HealthStatus.Unhealthy, ["readiness"])
        });

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenErrorCountAtOrAboveThreshold_ReturnsUnhealthy()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
        var tracker = new ErrorRateTracker(timeProvider);
        var options = Options.Create(new ErrorRateHealthCheckOptions { MaxErrorCount = 3 });
        var healthCheck = new ErrorRateHealthCheck(tracker, options);

        tracker.RecordServerError();
        tracker.RecordServerError();
        tracker.RecordServerError();

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("error-rate", healthCheck, HealthStatus.Unhealthy, ["readiness"])
        });

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
