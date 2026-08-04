using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Mova.Application.Health;
using Mova.Infrastructure.Configuration;
using Mova.Infrastructure.HealthChecks;

namespace Mova.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .Validate(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    return false;
                }

                var builder = new NpgsqlConnectionStringBuilder(options.ConnectionString);
                return !string.IsNullOrWhiteSpace(builder.Host);
            }, "A valid Database:ConnectionString is required.")
            .ValidateOnStart();

        services.AddSingleton<NpgsqlDataSource>(serviceProvider =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var builder = new NpgsqlConnectionStringBuilder(databaseOptions.ConnectionString);
            return NpgsqlDataSource.Create(builder.ConnectionString);
        });

        services.AddSingleton<IDatabaseConnectionProbe, NpgsqlDatabaseConnectionProbe>();

        return services;
    }
}
