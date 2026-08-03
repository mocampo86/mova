using ReservaCanchas.Application.Common.Errors;
using Xunit;

namespace ReservaCanchas.UnitTests.Application.Common.Errors;

public class ErrorResponseBuilderTests
{
    [Fact]
    public void Build_WhenIsDevelopmentIsFalse_ReturnsGenericMessageAndNoDetails()
    {
        var exception = new InvalidOperationException("Sensitive internal detail");

        var response = ErrorResponseBuilder.Build(exception, "trace-123", isDevelopment: false);

        Assert.Equal("INTERNAL_SERVER_ERROR", response.Error.Code);
        Assert.Equal("An unexpected error occurred. Please try again later.", response.Error.Message);
        Assert.Equal("trace-123", response.Error.TraceId);
        Assert.Null(response.Error.Details);
    }

    [Fact]
    public void Build_WhenIsDevelopmentIsTrue_ReturnsExceptionMessageAndDetails()
    {
        var exception = new InvalidOperationException("Sensitive internal detail");

        var response = ErrorResponseBuilder.Build(exception, "trace-abc", isDevelopment: true);

        Assert.Equal("INTERNAL_SERVER_ERROR", response.Error.Code);
        Assert.Equal("Sensitive internal detail", response.Error.Message);
        Assert.Equal("trace-abc", response.Error.TraceId);
        Assert.NotNull(response.Error.Details);
        Assert.Contains("exceptionType", response.Error.Details!.Keys);
        Assert.Contains("stackTrace", response.Error.Details.Keys);
    }
}
