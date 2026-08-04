using Npgsql;
using Mova.Application.Health;

namespace Mova.Infrastructure.HealthChecks;

public sealed class NpgsqlDatabaseConnectionProbe : IDatabaseConnectionProbe
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlDatabaseConnectionProbe(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<bool> IsConnectionHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
