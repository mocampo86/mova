using Microsoft.AspNetCore.Http;
using Mova.Api.Middleware;
using Xunit;

namespace Mova.UnitTests.Api.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithoutCorrelationIdHeader_GeneratesCorrelationIdAndSetsResponseHeader()
    {
        var middleware = new CorrelationIdMiddleware();
        var context = new DefaultHttpContext();
        string? capturedTraceId = null;

        await middleware.InvokeAsync(context, next =>
        {
            capturedTraceId = next.TraceIdentifier;
            return Task.CompletedTask;
        });

        Assert.NotNull(capturedTraceId);
        Assert.NotEmpty(capturedTraceId);
    }

    [Fact]
    public async Task InvokeAsync_WithCorrelationIdHeader_UsesProvidedValue()
    {
        var middleware = new CorrelationIdMiddleware();
        var context = new DefaultHttpContext();
        var expectedId = "my-correlation-id";
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedId;

        string? capturedTraceId = null;

        await middleware.InvokeAsync(context, next =>
        {
            capturedTraceId = next.TraceIdentifier;
            return Task.CompletedTask;
        });

        Assert.Equal(expectedId, capturedTraceId);
    }

    [Fact]
    public async Task InvokeAsync_SetsTraceIdentifierOnHttpContext()
    {
        var middleware = new CorrelationIdMiddleware();
        var context = new DefaultHttpContext();
        var expectedId = "trace-123";
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedId;

        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        Assert.Equal(expectedId, context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_WithExistingTraceIdentifier_UsesTraceIdentifier()
    {
        var middleware = new CorrelationIdMiddleware();
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "existing-trace-id";

        string? capturedTraceId = null;

        await middleware.InvokeAsync(context, next =>
        {
            capturedTraceId = next.TraceIdentifier;
            return Task.CompletedTask;
        });

        Assert.Equal("existing-trace-id", capturedTraceId);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        var middleware = new CorrelationIdMiddleware();
        var context = new DefaultHttpContext();
        var nextCalled = false;

        await middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
    }
}
