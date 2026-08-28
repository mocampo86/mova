using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mova.Api.HealthChecks;
using Mova.Tests.Common.Health;

namespace Mova.UnitTests.Health;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsHealthy_ReturnsHealthy()
    {
        var probe = new FakeDatabaseConnectionProbe(isHealthy: true);
        var check = new DatabaseHealthCheck(probe);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), default);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsUnhealthy_ReturnsUnhealthy()
    {
        var probe = new FakeDatabaseConnectionProbe(isHealthy: false);
        var check = new DatabaseHealthCheck(probe);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), default);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("Database is not reachable", result.Description);
    }
}
