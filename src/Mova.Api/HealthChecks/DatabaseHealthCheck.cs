using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mova.Application.Health;

namespace Mova.Api.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDatabaseConnectionProbe _probe;

    public DatabaseHealthCheck(IDatabaseConnectionProbe probe)
    {
        _probe = probe;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var isHealthy = await _probe.IsConnectionHealthyAsync(cancellationToken);

        return isHealthy
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Database is not reachable");
    }
}
