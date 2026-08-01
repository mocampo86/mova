using ReservaCanchas.Api.Exceptions;
using ReservaCanchas.Infrastructure;
using ReservaCanchas.Infrastructure.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace ReservaCanchas.Api;

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
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        var app = builder.Build();

        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();

#if DEBUG
        app.MapGet("/api/test/throw", static (HttpContext _) => throw new InvalidOperationException("Test exception"));
#endif

        app.Run();
    }
}
