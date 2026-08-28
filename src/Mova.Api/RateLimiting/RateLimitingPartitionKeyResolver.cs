using System.Security.Claims;

namespace Mova.Api.RateLimiting;

public static class RateLimitingPartitionKeyResolver
{
    public static string ResolveLoginPartitionKey(HttpContext httpContext)
    {
        var clientIp = ResolveClientIp(httpContext);
        return clientIp is not null ? $"ip:{clientIp}" : "anonymous";
    }

    public static string ResolveAuthenticatedPartitionKey(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContext.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        var clientIp = ResolveClientIp(httpContext);
        return clientIp is not null ? $"ip:{clientIp}" : "anonymous";
    }

    private static string? ResolveClientIp(HttpContext httpContext)
    {
        // HttpContext.Connection.RemoteIpAddress is the effective client IP
        // after the ForwardedHeaders middleware has processed trusted proxies.
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
