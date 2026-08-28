using Mova.Api.HealthChecks;

namespace Mova.UnitTests.Api.Health;

public class ErrorRateTrackerTests
{
    [Fact]
    public void RecordServerError_IncreasesErrorCount()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
        var tracker = new ErrorRateTracker(timeProvider);

        tracker.RecordServerError();
        tracker.RecordServerError();

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(2, snapshot.ErrorCount);
    }

    [Fact]
    public void GetSnapshot_AfterWindowSlides_DropsOldErrors()
    {
        var now = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(now);
        var tracker = new ErrorRateTracker(timeProvider);

        tracker.RecordServerError();

        timeProvider.Advance(TimeSpan.FromMinutes(6));

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(0, snapshot.ErrorCount);
    }

    [Fact]
    public void GetSnapshot_ErrorsWithinWindow_Remain()
    {
        var now = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(now);
        var tracker = new ErrorRateTracker(timeProvider);

        tracker.RecordServerError();

        timeProvider.Advance(TimeSpan.FromMinutes(2));

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(1, snapshot.ErrorCount);
    }
}
