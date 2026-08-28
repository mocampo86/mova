using System.Text;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mova.Api.Authorization;
using Mova.Api.Exceptions;
using Mova.Api.HealthChecks;
using Mova.Api.Middleware;
using Mova.Application;
using Mova.Infrastructure;
using Mova.Infrastructure.Authentication.Options;
using Mova.Infrastructure.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.ApplicationInsights;

namespace Mova.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, _, loggerConfiguration) =>
        {
            var isDevelopment = context.HostingEnvironment.IsDevelopment();

            var appInsightsConnectionString =
                context.Configuration["ApplicationInsights:ConnectionString"]
                ?? context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            loggerConfiguration
                .MinimumLevel.Is(isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Mova.Api")
                .WriteTo.Console(new SensitiveDataRedactingFormatter(new CompactJsonFormatter()));

            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
            {
                loggerConfiguration.WriteTo.ApplicationInsights(
                    appInsightsConnectionString,
                    TelemetryConverter.Traces,
                    restrictedToMinimumLevel: LogEventLevel.Information);
            }
        });

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.Configure<ErrorRateHealthCheckOptions>(
            builder.Configuration.GetSection(ErrorRateHealthCheckOptions.SectionName));

        builder.Services.AddSingleton<IErrorRateTracker, ErrorRateTracker>();
        builder.Services.AddScoped<ErrorRateTrackingMiddleware>();
        builder.Services.AddScoped<CorrelationIdMiddleware>();

        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["readiness"])
            .AddCheck<ErrorRateHealthCheck>("error-rate", tags: ["readiness"]);

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
        {
            allowedOrigins = ["http://localhost:5173"];
        }

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.SetIsOriginAllowed(_ => false);
                }

                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((jwtBearerOptions, jwtOptionsAccessor) =>
            {
                var jwtOptions = jwtOptionsAccessor.Value;
                var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

                jwtBearerOptions.MapInboundClaims = false;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    NameClaimType = "sub",
                    RoleClaimType = "roles"
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.User, policy => policy.RequireAuthenticatedUser());
            options.AddPolicy(AuthorizationPolicies.ComplexAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ComplexAdminRequirement());
            });
            options.AddPolicy(AuthorizationPolicies.SuperAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AuthorizationPolicies.SuperAdmin);
            });
        });

        builder.Services.AddScoped<IAuthorizationHandler, ComplexAdminAuthorizationHandler>();

        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("search", context =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst("sub")?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    userId,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 60,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.AddPolicy("login", context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 20,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.AddPolicy("reservation", context =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst("sub")?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    userId,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 30,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseMiddleware<ErrorRateTrackingMiddleware>();
        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        var rateLimitingEnabled = app.Services.GetRequiredService<IConfiguration>().GetValue("RateLimiting:Enabled", true);
        if (rateLimitingEnabled)
        {
            app.UseRateLimiter();
        }

        app.MapOpenApi();
        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        }).AllowAnonymous();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        }).AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("readiness"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        }).AllowAnonymous();

#if DEBUG
        app.MapGet("/api/test/throw", static (HttpContext _) => throw new InvalidOperationException("Test exception"));
#endif

        app.Run();
    }
}
