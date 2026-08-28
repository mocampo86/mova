using Mova.Application.Health;

namespace Mova.Tests.Common.Health;

public sealed class FakeDatabaseConnectionProbe : IDatabaseConnectionProbe
{
    private readonly bool _isHealthy;

    public FakeDatabaseConnectionProbe(bool isHealthy = true)
    {
        _isHealthy = isHealthy;
    }

    public Task<bool> IsConnectionHealthyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isHealthy);
    }
}
