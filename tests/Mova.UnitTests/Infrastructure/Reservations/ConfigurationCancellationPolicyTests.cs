using Microsoft.Extensions.Options;
using Mova.Infrastructure.Configuration;
using Mova.Infrastructure.Reservations;
using Xunit;

namespace Mova.UnitTests.Infrastructure.Reservations;

public sealed class ConfigurationCancellationPolicyTests
{
    [Fact]
    public void IsWithinCancellationWindow_WhenAllowedAndBeforeDeadline_ReturnsTrue()
    {
        var policy = CreatePolicy(24, true);
        var startAt = DateTime.UtcNow.AddDays(2);

        Assert.True(policy.IsWithinCancellationWindow(startAt, DateTime.UtcNow));
    }

    [Fact]
    public void IsWithinCancellationWindow_WhenAllowedAndAfterDeadline_ReturnsFalse()
    {
        var policy = CreatePolicy(24, true);
        var startAt = DateTime.UtcNow.AddHours(1);

        Assert.False(policy.IsWithinCancellationWindow(startAt, DateTime.UtcNow));
    }

    [Fact]
    public void IsWithinCancellationWindow_WhenNotAllowed_ReturnsFalse()
    {
        var policy = CreatePolicy(0, false);
        var startAt = DateTime.UtcNow.AddDays(7);

        Assert.False(policy.IsWithinCancellationWindow(startAt, DateTime.UtcNow));
    }

    private static ConfigurationCancellationPolicy CreatePolicy(int minimumHours, bool allowUserCancellation)
    {
        var options = new CancellationPolicyOptions
        {
            MinimumHours = minimumHours,
            AllowUserCancellation = allowUserCancellation
        };

        return new ConfigurationCancellationPolicy(Options.Create(options));
    }
}
