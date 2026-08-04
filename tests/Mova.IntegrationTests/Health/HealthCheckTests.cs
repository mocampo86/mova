using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mova.Api;
using Mova.Application.Health;

namespace Mova.IntegrationTests.Health;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Liveness_Returns_Healthy_200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await ParseResponseAsync(response);
        var root = document.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.Empty(root.GetProperty("dependencies").EnumerateArray());
    }

    [Fact]
    public async Task Readiness_WhenDatabaseIsHealthy_Returns_Healthy_200()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDatabaseConnectionProbe>(new FakeDatabaseConnectionProbe(isHealthy: true));
            });
        });

        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await ParseResponseAsync(response);
        var dependency = document.RootElement.GetProperty("dependencies")[0];

        Assert.Equal("database", dependency.GetProperty("name").GetString());
        Assert.Equal("Healthy", dependency.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Readiness_WhenDatabaseIsUnhealthy_Returns_Unhealthy_503()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDatabaseConnectionProbe>(new FakeDatabaseConnectionProbe(isHealthy: false));
            });
        });

        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        using var document = await ParseResponseAsync(response);
        var root = document.RootElement;

        Assert.Equal("Unhealthy", root.GetProperty("status").GetString());
        var dependency = root.GetProperty("dependencies")[0];

        Assert.Equal("database", dependency.GetProperty("name").GetString());
        Assert.Equal("Unhealthy", dependency.GetProperty("status").GetString());
    }

    private static async Task<JsonDocument> ParseResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
