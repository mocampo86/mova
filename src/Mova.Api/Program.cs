using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mova.Api.Exceptions;
using Mova.Api.HealthChecks;
using Mova.Infrastructure;
using Mova.Infrastructure.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Mova.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, _, loggerConfiguration) =>
        {
            var isDevelopment = context.HostingEnvironment.IsDevelopment();

            loggerConfiguration
                .MinimumLevel.Is(isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(new SensitiveDataRedactingFormatter(new CompactJsonFormatter()));
        });

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["readiness"]);
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        var app = builder.Build();

        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();

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
