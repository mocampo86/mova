namespace Mova.Application.Abstractions.Policies;

public interface ICancellationPolicy
{
    Task<CancellationPolicyValues> GetSettingsAsync(Guid sportsComplexId, CancellationToken cancellationToken = default);

    Task<CancellationPolicyResult> EvaluateAsync(Guid sportsComplexId, DateTime startAt, DateTime now, CancellationToken cancellationToken = default);
}

public sealed record CancellationPolicyValues(int MinimumHours, bool AllowUserCancellation);

public sealed record CancellationPolicyResult(bool AllowUserCancellation, bool IsWithinWindow, int MinimumHours);
