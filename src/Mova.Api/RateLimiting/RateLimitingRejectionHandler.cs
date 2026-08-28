using System.Diagnostics;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Mova.Contracts.Errors;

namespace Mova.Api.RateLimiting;

public static class RateLimitingRejectionHandler
{
    public static async ValueTask HandleRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var response = new ApiErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = "RATE_LIMIT_EXCEEDED",
                Message = "Too many requests. Please try again later.",
                TraceId = traceId
            }
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    }
}
