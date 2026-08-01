using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReservaCanchas.Api;
using ReservaCanchas.Infrastructure.Configuration;
using Xunit;

namespace ReservaCanchas.IntegrationTests.Configuration;

public class ConfigurationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConfigurationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Backend_Startup_BindsDatabaseOptions()
    {
        var options = _factory.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        Assert.NotNull(options);
        Assert.NotEmpty(options.ConnectionString);
    }

    [Fact]
    public void Backend_Startup_Throws_WhenDatabaseConnectionStringMissing()
    {
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string?>());
            });
        });

        var exception = Assert.ThrowsAny<Exception>(() => { _ = customFactory.Services; });

        Assert.Contains("Database:ConnectionString", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
