using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Abstractions.Policies;
using Mova.Application.Health;
using Mova.Infrastructure.Authentication;
using Mova.Infrastructure.Authentication.Options;
using Mova.Infrastructure.Configuration;
using Mova.Infrastructure.Data;
using Mova.Infrastructure.HealthChecks;
using Mova.Infrastructure.Persistence;
using Mova.Infrastructure.Persistence.Repositories;
using Mova.Infrastructure.Reservations;

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

        services.AddDbContext<MovaDbContext>((serviceProvider, options) =>
        {
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
            options.UseNpgsql(dataSource);
        });

        services.AddSingleton<IDatabaseConnectionProbe, NpgsqlDatabaseConnectionProbe>();

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(options =>
            {
                if (string.IsNullOrWhiteSpace(options.SecretKey))
                {
                    return false;
                }

                return Encoding.UTF8.GetBytes(options.SecretKey).Length >= 32;
            }, "Jwt:SecretKey must be at least 32 bytes long.");

        services.AddOptions<GoogleAuthOptions>()
            .BindConfiguration(GoogleAuthOptions.SectionName);

        services.AddOptions<CancellationPolicyOptions>()
            .BindConfiguration(CancellationPolicyOptions.SectionName)
            .Validate(options => options.MinimumHours >= 0, "CancellationPolicy:MinimumHours must be greater than or equal to 0.");

        services.AddScoped<ICancellationPolicy, ConfigurationCancellationPolicy>();

        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISportsComplexRepository, SportsComplexRepository>();
        services.AddScoped<IComplexAdministratorRepository, ComplexAdministratorRepository>();
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<ISportRepository, SportRepository>();
        services.AddScoped<ICourtAvailabilityRuleRepository, CourtAvailabilityRuleRepository>();
        services.AddScoped<IBusinessHoursRepository, BusinessHoursRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ICourtBlockRepository, CourtBlockRepository>();
        services.AddScoped<IBlockedUserRepository, BlockedUserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
