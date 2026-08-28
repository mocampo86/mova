using Mova.Api.HealthChecks;

namespace Mova.IntegrationTests.Health;

public sealed class FakeErrorRateTracker : IErrorRateTracker
{
    private readonly ErrorRateSnapshot _snapshot;

    public FakeErrorRateTracker(int errorCount = 0)
    {
        _snapshot = new ErrorRateSnapshot(errorCount, TimeSpan.FromMinutes(5), 0);
    }

    public void RecordServerError()
    {
    }

    public ErrorRateSnapshot GetSnapshot() => _snapshot;
}
