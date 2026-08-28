using Microsoft.Extensions.Logging;
using Npgsql;
using Mova.Application.Health;

namespace Mova.Infrastructure.HealthChecks;

public sealed class NpgsqlDatabaseConnectionProbe : IDatabaseConnectionProbe
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<NpgsqlDatabaseConnectionProbe> _logger;

    public NpgsqlDatabaseConnectionProbe(NpgsqlDataSource dataSource, ILogger<NpgsqlDatabaseConnectionProbe> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<bool> IsConnectionHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed: the database is not reachable.");
            return false;
        }
    }
}
