using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mova.Api.Authorization;
using Mova.Api.Configuration;
using Mova.Api.Exceptions;
using Mova.Api.HealthChecks;
using Mova.Api.Middleware;
using Mova.Api.RateLimiting;
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

        builder.Services.Configure<ErrorRateTrackerOptions>(
            builder.Configuration.GetSection(ErrorRateTrackerOptions.SectionName));

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

        builder.Services.Configure<RateLimitingOptions>(
            builder.Configuration.GetSection(RateLimitingOptions.SectionName));

        builder.Services.Configure<ForwardedHeadersSettings>(
            builder.Configuration.GetSection(ForwardedHeadersSettings.SectionName));

        builder.Services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<ForwardedHeadersSettings>>((options, settingsAccessor) =>
            {
                var settings = settingsAccessor.Value;

                if (Enum.TryParse<Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders>(settings.ForwardedHeaders, true, out var forwardedHeaders))
                {
                    options.ForwardedHeaders = forwardedHeaders;
                }
                else
                {
                    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor;
                }

                options.ForwardLimit = settings.ForwardLimit ?? 1;

                foreach (var proxy in settings.KnownProxies)
                {
                    if (IPAddress.TryParse(proxy, out var ip))
                    {
                        options.KnownProxies.Add(ip);
                    }
                }

                foreach (var network in settings.KnownNetworks)
                {
                    if (System.Net.IPNetwork.TryParse(network, out var ipNetwork))
                    {
                        options.KnownIPNetworks.Add(ipNetwork);
                    }
                }
            });

        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("search", context =>
            {
                var rateOptions = context.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    RateLimitingPartitionKeyResolver.ResolveAuthenticatedPartitionKey(context),
                    _ => CreateFixedWindowOptions(rateOptions.Search));
            });

            options.AddPolicy("login", context =>
            {
                var rateOptions = context.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    RateLimitingPartitionKeyResolver.ResolveLoginPartitionKey(context),
                    _ => CreateFixedWindowOptions(rateOptions.Login));
            });

            options.AddPolicy("reservation", context =>
            {
                var rateOptions = context.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    RateLimitingPartitionKeyResolver.ResolveAuthenticatedPartitionKey(context),
                    _ => CreateFixedWindowOptions(rateOptions.Reservation));
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = RateLimitingRejectionHandler.HandleRejectionAsync;
        });

        static FixedWindowRateLimiterOptions CreateFixedWindowOptions(RateLimitingPolicyOptions policyOptions) =>
            new()
            {
                AutoReplenishment = true,
                PermitLimit = policyOptions.PermitLimit,
                QueueLimit = policyOptions.QueueLimit,
                Window = TimeSpan.FromSeconds(policyOptions.WindowSeconds)
            };

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ErrorRateTrackingMiddleware>();
        app.UseExceptionHandler();
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();

        app.UseCors();

        var rateLimitingOptions = app.Services.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

        app.UseAuthentication();

        if (rateLimitingOptions.Enabled)
        {
            app.UseRateLimiter();
        }

        app.UseAuthorization();

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
