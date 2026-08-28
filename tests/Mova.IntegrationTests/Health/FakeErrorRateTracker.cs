using Mova.Api.HealthChecks;

namespace Mova.IntegrationTests.Health;

public sealed class FakeErrorRateTracker : IErrorRateTracker
{
    private readonly ErrorRateSnapshot _snapshot;

    public FakeErrorRateTracker(int errorCount = 0)
    {
        var window = TimeSpan.FromMinutes(5);
        var rate = errorCount / window.TotalMinutes;
        _snapshot = new ErrorRateSnapshot(errorCount, window, rate);
    }

    public void RecordServerError()
    {
    }

    public ErrorRateSnapshot GetSnapshot() => _snapshot;
}
