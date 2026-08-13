using FluentValidation;
using Mova.Application.Common.Exceptions;
using Mova.Contracts.Errors;
using Mova.Domain.Exceptions;

namespace Mova.Application.Common.Errors;

public static class ErrorResponseBuilder
{
    public const string DefaultErrorCode = "INTERNAL_SERVER_ERROR";
    public const string DefaultProductionMessage = "An unexpected error occurred. Please try again later.";

    public static ApiErrorResponse Build(Exception exception, string traceId, bool isDevelopment)
    {
        return BuildResponse(exception, traceId, isDevelopment).Response;
    }

    public static (ApiErrorResponse Response, int StatusCode) BuildResponse(Exception exception, string traceId, bool isDevelopment)
    {
        int statusCode;
        string code;
        string message;
        Dictionary<string, object?>? details = null;

        switch (exception)
        {
            case AuthenticationException:
                statusCode = 401;
                code = "UNAUTHORIZED";
                message = exception.Message;
                break;

            case UserBlockedException:
                statusCode = 403;
                code = "USER_BLOCKED";
                message = exception.Message;
                break;

            case NotFoundException:
                statusCode = 404;
                code = "NOT_FOUND";
                message = exception.Message;
                break;

            case CancellationDeadlineExceededException:
                statusCode = 409;
                code = "CANCELLATION_DEADLINE_PASSED";
                message = exception.Message;
                break;

            case ConflictException:
                statusCode = 409;
                code = "CONFLICT";
                message = exception.Message;
                break;

            case ValidationException validationException:
                statusCode = 400;
                code = "VALIDATION_ERROR";
                message = "One or more validation errors occurred.";
                details = new Dictionary<string, object?>
                {
                    ["errors"] = validationException.Errors
                        .Select(e => new { propertyName = e.PropertyName, message = e.ErrorMessage })
                        .ToArray()
                };
                break;

            default:
                statusCode = 500;
                code = DefaultErrorCode;
                message = isDevelopment ? exception.Message : DefaultProductionMessage;
                if (isDevelopment)
                {
                    details = new Dictionary<string, object?>
                    {
                        ["exceptionType"] = exception.GetType().FullName,
                        ["stackTrace"] = exception.StackTrace
                    };
                }

                break;
        }

        var response = new ApiErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = code,
                Message = message,
                Details = details,
                TraceId = traceId
            }
        };

        return (response, statusCode);
    }
}
