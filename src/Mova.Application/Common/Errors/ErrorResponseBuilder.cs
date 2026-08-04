using Mova.Contracts.Errors;

namespace Mova.Application.Common.Errors;

public static class ErrorResponseBuilder
{
    public const string DefaultErrorCode = "INTERNAL_SERVER_ERROR";
    public const string DefaultProductionMessage = "An unexpected error occurred. Please try again later.";

    public static ApiErrorResponse Build(Exception exception, string traceId, bool isDevelopment)
    {
        var message = isDevelopment
            ? exception.Message
            : DefaultProductionMessage;

        Dictionary<string, object?>? details = null;
        if (isDevelopment)
        {
            details = new Dictionary<string, object?>
            {
                ["exceptionType"] = exception.GetType().FullName,
                ["stackTrace"] = exception.StackTrace
            };
        }

        return new ApiErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = DefaultErrorCode,
                Message = message,
                Details = details,
                TraceId = traceId
            }
        };
    }
}
