using Mova.Application.Abstractions.Policies;

namespace Mova.UnitTests.Application.Reservations;

public sealed class FakeCancellationPolicy(int minimumHours, bool allowUserCancellation) : ICancellationPolicy
{
    public Task<CancellationPolicyValues> GetSettingsAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CancellationPolicyValues(minimumHours, allowUserCancellation));
    }

    public Task<CancellationPolicyResult> EvaluateAsync(Guid sportsComplexId, DateTime startAt, DateTime now, CancellationToken cancellationToken = default)
    {
        if (!allowUserCancellation)
        {
            return Task.FromResult(new CancellationPolicyResult(false, false, minimumHours));
        }

        var isWithinWindow = now <= startAt.AddHours(-minimumHours);
        return Task.FromResult(new CancellationPolicyResult(true, isWithinWindow, minimumHours));
    }
}
