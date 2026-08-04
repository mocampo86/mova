using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Mova.Api.HealthChecks;

namespace Mova.UnitTests.Health;

public class HealthCheckResponseWriterTests
{
    private static HealthReport CreateReport(string environmentName = "Production")
    {
        var exception = environmentName == "Production" ? null : new InvalidOperationException("Sensitive details");
        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new(
                status: HealthStatus.Healthy,
                description: "All good",
                duration: TimeSpan.FromMilliseconds(12),
                exception: exception,
                data: null,
                tags: ["readiness"])
        };

        return new HealthReport(entries, TimeSpan.FromMilliseconds(20));
    }

    private static DefaultHttpContext CreateHttpContext(string environmentName = "Production")
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment { EnvironmentName = environmentName });
        var context = new DefaultHttpContext();
        context.RequestServices = services.BuildServiceProvider();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task WriteResponseAsync_InProduction_WritesStatusAndDependenciesWithoutDescriptions()
    {
        var context = CreateHttpContext("Production");
        var report = CreateReport("Production");

        await HealthCheckResponseWriter.WriteResponseAsync(context, report);

        var json = await ReadBodyAsync(context.Response);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());

        var dependency = root.GetProperty("dependencies")[0];
        Assert.Equal("database", dependency.GetProperty("name").GetString());
        Assert.Equal("Healthy", dependency.GetProperty("status").GetString());
        Assert.False(dependency.TryGetProperty("description", out _));
        Assert.False(dependency.TryGetProperty("exception", out _));
    }

    [Fact]
    public async Task WriteResponseAsync_InDevelopment_IncludesDescriptions()
    {
        var context = CreateHttpContext("Development");
        var report = CreateReport("Development");

        await HealthCheckResponseWriter.WriteResponseAsync(context, report);

        var json = await ReadBodyAsync(context.Response);
        using var document = JsonDocument.Parse(json);
        var dependency = document.RootElement.GetProperty("dependencies")[0];

        Assert.Equal("All good", dependency.GetProperty("description").GetString());
        Assert.False(dependency.TryGetProperty("exception", out _));
    }

    private static async Task<string> ReadBodyAsync(HttpResponse response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Production";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(Path.GetTempPath());
    }
}
