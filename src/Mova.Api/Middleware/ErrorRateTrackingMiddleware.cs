using Mova.Api.HealthChecks;

namespace Mova.Api.Middleware;

public sealed class ErrorRateTrackingMiddleware : IMiddleware
{
    private readonly IErrorRateTracker _tracker;

    public ErrorRateTrackingMiddleware(IErrorRateTracker tracker)
    {
        _tracker = tracker;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        finally
        {
            if (context.Response.StatusCode >= 500)
            {
                _tracker.RecordServerError();
            }
        }
    }
}
