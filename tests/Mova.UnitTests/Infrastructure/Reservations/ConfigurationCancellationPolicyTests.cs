using Microsoft.Extensions.Options;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Abstractions.Policies;
using Mova.Domain.Entities;
using Mova.Infrastructure.Configuration;
using Mova.Infrastructure.Reservations;
using Xunit;

namespace Mova.UnitTests.Infrastructure.Reservations;

public sealed class ConfigurationCancellationPolicyTests
{
    [Fact]
    public async Task GetSettingsAsync_WhenConfigured_ReturnsConfiguredValues()
    {
        var repository = new FakeCancellationPolicyRepository();
        var complexId = Guid.NewGuid();
        await repository.AddAsync(CancellationPolicy.Create(complexId, 12, false));

        var policy = CreatePolicy(repository, 24, true);

        var settings = await policy.GetSettingsAsync(complexId);

        Assert.Equal(12, settings.MinimumHours);
        Assert.False(settings.AllowUserCancellation);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenNotConfigured_FallsBackToOptions()
    {
        var repository = new FakeCancellationPolicyRepository();

        var policy = CreatePolicy(repository, 48, true);

        var settings = await policy.GetSettingsAsync(Guid.NewGuid());

        Assert.Equal(48, settings.MinimumHours);
        Assert.True(settings.AllowUserCancellation);
    }

    [Fact]
    public async Task EvaluateAsync_WhenAllowedAndBeforeDeadline_ReturnsTrue()
    {
        var policy = CreatePolicy(new FakeCancellationPolicyRepository(), 24, true);
        var startAt = DateTime.UtcNow.AddDays(2);

        var result = await policy.EvaluateAsync(Guid.NewGuid(), startAt, DateTime.UtcNow);

        Assert.True(result.AllowUserCancellation);
        Assert.True(result.IsWithinWindow);
        Assert.Equal(24, result.MinimumHours);
    }

    [Fact]
    public async Task EvaluateAsync_WhenAllowedAndAfterDeadline_ReturnsFalse()
    {
        var policy = CreatePolicy(new FakeCancellationPolicyRepository(), 24, true);
        var startAt = DateTime.UtcNow.AddHours(1);

        var result = await policy.EvaluateAsync(Guid.NewGuid(), startAt, DateTime.UtcNow);

        Assert.True(result.AllowUserCancellation);
        Assert.False(result.IsWithinWindow);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNotAllowed_ReturnsFalse()
    {
        var policy = CreatePolicy(new FakeCancellationPolicyRepository(), 0, false);
        var startAt = DateTime.UtcNow.AddDays(7);

        var result = await policy.EvaluateAsync(Guid.NewGuid(), startAt, DateTime.UtcNow);

        Assert.False(result.AllowUserCancellation);
        Assert.False(result.IsWithinWindow);
    }

    private static ConfigurationCancellationPolicy CreatePolicy(
        ICancellationPolicyRepository repository,
        int minimumHours,
        bool allowUserCancellation)
    {
        var options = new CancellationPolicyOptions
        {
            MinimumHours = minimumHours,
            AllowUserCancellation = allowUserCancellation
        };

        return new ConfigurationCancellationPolicy(Options.Create(options), repository);
    }

    private sealed class FakeCancellationPolicyRepository : ICancellationPolicyRepository
    {
        private readonly List<CancellationPolicy> _policies = [];

        public Task<CancellationPolicy?> GetBySportsComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_policies.FirstOrDefault(p => p.SportsComplexId == sportsComplexId));
        }

        public Task AddAsync(CancellationPolicy cancellationPolicy, CancellationToken cancellationToken = default)
        {
            _policies.Add(cancellationPolicy);
            return Task.CompletedTask;
        }
    }
}
