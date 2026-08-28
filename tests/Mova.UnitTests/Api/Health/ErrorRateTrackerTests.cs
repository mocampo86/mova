using Microsoft.Extensions.Options;
using Mova.Api.HealthChecks;

namespace Mova.UnitTests.Api.Health;

public class ErrorRateTrackerTests
{
    private static ErrorRateTracker CreateTracker(FakeTimeProvider timeProvider, int maxQueueSize = 1000)
    {
        var options = Options.Create(new ErrorRateTrackerOptions
        {
            EvaluationWindow = TimeSpan.FromMinutes(5),
            MaxQueueSize = maxQueueSize
        });
        return new ErrorRateTracker(timeProvider, options);
    }

    [Fact]
    public void RecordServerError_IncreasesErrorCount()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
        var tracker = CreateTracker(timeProvider);

        tracker.RecordServerError();
        tracker.RecordServerError();

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(2, snapshot.ErrorCount);
        Assert.Equal(0.4, snapshot.ErrorRatePerMinute, precision: 5);
    }

    [Fact]
    public void GetSnapshot_AfterWindowSlides_DropsOldErrors()
    {
        var now = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(now);
        var tracker = CreateTracker(timeProvider);

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
        var tracker = CreateTracker(timeProvider);

        tracker.RecordServerError();

        timeProvider.Advance(TimeSpan.FromMinutes(2));

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(1, snapshot.ErrorCount);
    }

    [Fact]
    public void RecordServerError_WhenMaxQueueSizeExceeded_DropsOldest()
    {
        var now = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(now);
        var tracker = CreateTracker(timeProvider, maxQueueSize: 2);

        tracker.RecordServerError();
        tracker.RecordServerError();
        tracker.RecordServerError();

        var snapshot = tracker.GetSnapshot();

        Assert.Equal(2, snapshot.ErrorCount);
    }
}
