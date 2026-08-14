using Microsoft.Extensions.Options;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Abstractions.Policies;
using Mova.Infrastructure.Configuration;

namespace Mova.Infrastructure.Reservations;

public sealed class ConfigurationCancellationPolicy(
    IOptions<CancellationPolicyOptions> options,
    ICancellationPolicyRepository repository) : ICancellationPolicy
{
    public async Task<CancellationPolicyValues> GetSettingsAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        var configured = await repository.GetBySportsComplexIdAsync(sportsComplexId, cancellationToken);

        var minimumHours = configured?.MinimumHours ?? options.Value.MinimumHours;
        var allowUserCancellation = configured?.AllowUserCancellation ?? options.Value.AllowUserCancellation;

        return new CancellationPolicyValues(minimumHours, allowUserCancellation);
    }

    public async Task<CancellationPolicyResult> EvaluateAsync(Guid sportsComplexId, DateTime startAt, DateTime now, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(sportsComplexId, cancellationToken);

        if (!settings.AllowUserCancellation)
        {
            return new CancellationPolicyResult(false, false, settings.MinimumHours);
        }

        var deadline = startAt.AddHours(-settings.MinimumHours);
        var isWithinWindow = now <= deadline;

        return new CancellationPolicyResult(true, isWithinWindow, settings.MinimumHours);
    }
}
