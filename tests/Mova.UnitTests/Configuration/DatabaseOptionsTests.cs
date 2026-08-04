using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mova.Infrastructure.Configuration;
using Xunit;

namespace Mova.UnitTests.Configuration;

public class DatabaseOptionsTests
{
    [Fact]
    public void AddDatabaseOptions_BindsConnectionString_WhenConfigured()
    {
        var expected = "Host=localhost;Port=5432;Database=mova;Username=postgres";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{DatabaseOptions.SectionName}:ConnectionString"] = expected
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<DatabaseOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetSection(DatabaseOptions.SectionName)["ConnectionString"] ?? string.Empty;
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Database ConnectionString is required.");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        Assert.Equal(expected, options.ConnectionString);
    }

    [Fact]
    public void AddDatabaseOptions_Throws_WhenConnectionStringMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddOptions<DatabaseOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetSection(DatabaseOptions.SectionName)["ConnectionString"] ?? string.Empty;
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Database ConnectionString is required.")
            .ValidateOnStart();

        var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<DatabaseOptions>>().Value);
    }
}
