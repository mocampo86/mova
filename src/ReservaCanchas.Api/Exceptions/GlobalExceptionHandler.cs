using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using ReservaCanchas.Application.Common.Errors;

namespace ReservaCanchas.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IHostEnvironment environment, ILogger<GlobalExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

        var response = ErrorResponseBuilder.Build(exception, traceId, _environment.IsDevelopment());

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        try
        {
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        }
        catch (Exception writeException)
        {
            _logger.LogError(writeException, "Failed to write error response. TraceId: {TraceId}", traceId);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsync("An unexpected error occurred.", cancellationToken);
        }

        return true;
    }
}
