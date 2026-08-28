using System.Collections.Concurrent;

namespace Mova.Api.HealthChecks;

public sealed class ErrorRateTracker : IErrorRateTracker
{
    private readonly ConcurrentQueue<DateTimeOffset> _errors = new();
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _window;

    public ErrorRateTracker(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _window = TimeSpan.FromMinutes(5);
    }

    public void RecordServerError()
    {
        var now = _timeProvider.GetUtcNow();
        _errors.Enqueue(now);
        Trim(now);
    }

    public ErrorRateSnapshot GetSnapshot()
    {
        var now = _timeProvider.GetUtcNow();
        Trim(now);
        var count = _errors.Count;
        var rate = _window.TotalMinutes > 0 ? count / _window.TotalMinutes : 0;
        return new ErrorRateSnapshot(count, _window, rate);
    }

    private void Trim(DateTimeOffset now)
    {
        var cutoff = now - _window;
        while (_errors.TryPeek(out var timestamp) && timestamp < cutoff)
        {
            _errors.TryDequeue(out _);
        }
    }
}
