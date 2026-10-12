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

            case UserCancellationDisabledException:
                statusCode = 409;
                code = "USER_CANCELLATION_DISABLED";
                message = exception.Message;
                break;

            case UserRecurringReservationsDisabledException:
                statusCode = 409;
                code = "RECURRING_RESERVATIONS_DISABLED";
                message = exception.Message;
                break;

            case UnresolvedTimeZoneException:
                statusCode = 422;
                code = "TIMEZONE_NOT_CONFIGURED";
                message = exception.Message;
                break;

            case PriceNotConfiguredException priceNotConfiguredException:
                statusCode = 422;
                code = "PRICE_NOT_CONFIGURED";
                message = exception.Message;
                details = new Dictionary<string, object?>
                {
                    ["gaps"] = priceNotConfiguredException.Gaps
                        .Select(g => new { startAt = g.StartAt, endAt = g.EndAt })
                        .ToArray()
                };
                break;

            case RateOverlapException:
                statusCode = 409;
                code = "RATE_OVERLAP";
                message = exception.Message;
                break;

            case PriceChangedException priceChangedException:
                statusCode = 409;
                code = "PRICE_CHANGED";
                message = exception.Message;
                details = new Dictionary<string, object?>
                {
                    ["expectedTotalPrice"] = priceChangedException.ExpectedTotalPrice,
                    ["expectedCurrency"] = priceChangedException.ExpectedCurrency,
                    ["actualTotalPrice"] = priceChangedException.ActualTotalPrice,
                    ["actualCurrency"] = priceChangedException.ActualCurrency
                };
                break;

            case ConflictException:
                statusCode = 409;
                code = "CONFLICT";
                message = exception.Message;
                break;

            case ArgumentException argumentException:
                statusCode = 400;
                code = "VALIDATION_ERROR";
                message = argumentException.Message;
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
