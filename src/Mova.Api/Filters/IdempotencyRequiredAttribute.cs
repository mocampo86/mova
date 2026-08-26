using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Mova.Application.Common.Idempotency;

namespace Mova.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class IdempotencyRequiredAttribute : Attribute, IAsyncActionFilter
{
    public const string HeaderName = "Idempotency-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var store = context.HttpContext.RequestServices.GetRequiredService<IIdempotencyStore>();

        var idempotencyKey = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(idempotencyKey) || !Guid.TryParse(idempotencyKey, out _))
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = new
                {
                    code = "IDEMPOTENCY_KEY_REQUIRED",
                    message = $"A valid UUID '{HeaderName}' header is required."
                }
            });
            return;
        }

        var actorKey = context.HttpContext.User.Identity?.IsAuthenticated == true
            ? GetUserActorKey(context.HttpContext.User)
            : context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        var scope = $"{context.HttpContext.Request.Method}:{context.HttpContext.Request.Path}".ToLowerInvariant();

        var record = await store.GetAsync(actorKey, scope, idempotencyKey);
        if (record is not null)
        {
            context.HttpContext.Response.StatusCode = record.StatusCode;
            context.Result = new ContentResult
            {
                Content = record.ResponseBody,
                ContentType = "application/json",
                StatusCode = record.StatusCode
            };
            return;
        }

        var executed = await next();

        if (executed.Exception is null && executed.Result is not null)
        {
            var (statusCode, body) = ExtractResponse(executed.Result);
            if (statusCode.HasValue)
            {
                await store.StoreAsync(actorKey, scope, idempotencyKey, statusCode.Value, body ?? string.Empty);
            }
        }
    }

    private static string GetUserActorKey(System.Security.Claims.ClaimsPrincipal user)
    {
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
            ?? "unknown";

        return $"user:{userId}";
    }

    private static (int? StatusCode, string? Body) ExtractResponse(IActionResult result)
    {
        int? statusCode = result switch
        {
            ObjectResult matchedObjectResult => matchedObjectResult.StatusCode,
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
            ForbidResult => StatusCodes.Status403Forbidden,
            _ => null
        };

        if (statusCode is null)
        {
            return (null, null);
        }

        object? value = result is ObjectResult objectResult ? objectResult.Value : null;
        if (value is null)
        {
            return (statusCode, null);
        }

        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        return (statusCode, json);
    }
}
