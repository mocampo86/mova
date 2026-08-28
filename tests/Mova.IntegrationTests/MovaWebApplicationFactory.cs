using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mova.Api;
using Mova.Application.Abstractions.Authentication;
using Mova.Infrastructure.Data;
using Mova.IntegrationTests.Authentication;
using Npgsql;
using Xunit;

namespace Mova.IntegrationTests;

public sealed class MovaWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var tempConfig = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("Database__ConnectionString")
            ?? tempConfig["Database:ConnectionString"]
            ?? throw new InvalidOperationException(
                "Database:ConnectionString is not configured. Set the 'Database__ConnectionString' environment variable or add it to user secrets.");

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "mova_test"
        };

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = connectionStringBuilder.ConnectionString,
                ["Jwt:SecretKey"] = "test-secret-must-be-at-least-32-bytes-long!",
                ["Jwt:Issuer"] = "Mova",
                ["Jwt:Audience"] = "Mova.Api",
                ["Jwt:ExpirationMinutes"] = "15",
                ["Google:ClientId"] = "test-client-id.apps.googleusercontent.com",
                ["CancellationPolicy:MinimumHours"] = "24",
                ["CancellationPolicy:AllowUserCancellation"] = "true",
                ["RateLimiting:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped<IGoogleTokenValidator, FakeGoogleTokenValidator>());
            services.Replace(ServiceDescriptor.Singleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))));
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
        await context.Database.MigrateAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
