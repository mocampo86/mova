namespace Mova.Api.HealthChecks;

public interface IErrorRateTracker
{
    void RecordServerError();
    ErrorRateSnapshot GetSnapshot();
}

public sealed record ErrorRateSnapshot(int ErrorCount, TimeSpan Window, double ErrorRatePerMinute);
