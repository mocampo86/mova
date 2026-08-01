using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ReservaCanchas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
        }

        services.AddSingleton<NpgsqlDataSource>(_ =>
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return NpgsqlDataSource.Create(builder.ConnectionString);
        });

        return services;
    }
}
