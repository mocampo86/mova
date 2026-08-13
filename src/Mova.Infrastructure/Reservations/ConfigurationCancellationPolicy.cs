using Microsoft.Extensions.Options;
using Mova.Application.Abstractions.Policies;
using Mova.Infrastructure.Configuration;

namespace Mova.Infrastructure.Reservations;

public sealed class ConfigurationCancellationPolicy(IOptions<CancellationPolicyOptions> options) : ICancellationPolicy
{
    public bool IsWithinCancellationWindow(DateTime startAt, DateTime now)
    {
        if (!options.Value.AllowUserCancellation)
        {
            return false;
        }

        var deadline = startAt.AddHours(-options.Value.MinimumHours);
        return now <= deadline;
    }
}
