using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Mova.Api.HealthChecks;

public sealed class ErrorRateTracker : IErrorRateTracker
{
    private readonly ConcurrentQueue<DateTimeOffset> _errors = new();
    private readonly TimeProvider _timeProvider;
    private readonly ErrorRateTrackerOptions _options;

    public ErrorRateTracker(TimeProvider timeProvider, IOptions<ErrorRateTrackerOptions> options)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    public void RecordServerError()
    {
        var now = _timeProvider.GetUtcNow();
        _errors.Enqueue(now);
        Trim(now);

        while (_errors.Count > _options.MaxQueueSize)
        {
            _errors.TryDequeue(out _);
        }
    }

    public ErrorRateSnapshot GetSnapshot()
    {
        var now = _timeProvider.GetUtcNow();
        Trim(now);

        var count = _errors.Count;
        var windowMinutes = _options.EvaluationWindow.TotalMinutes;
        var rate = windowMinutes > 0 ? count / windowMinutes : 0;
        return new ErrorRateSnapshot(count, _options.EvaluationWindow, rate);
    }

    private void Trim(DateTimeOffset now)
    {
        var cutoff = now - _options.EvaluationWindow;
        while (_errors.TryPeek(out var timestamp) && timestamp < cutoff)
        {
            _errors.TryDequeue(out _);
        }
    }
}
