using Microsoft.AspNetCore.Http;
using Mova.Api.HealthChecks;
using Mova.Api.Middleware;
using Xunit;

namespace Mova.UnitTests.Api.Middleware;

public class ErrorRateTrackingMiddlewareTests
{
    private readonly FakeErrorRateTracker _tracker = new();

    [Fact]
    public async Task InvokeAsync_WhenResponseStatusIs500_RecordsServerError()
    {
        var middleware = new ErrorRateTrackingMiddleware(_tracker);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, ctx =>
        {
            ctx.Response.StatusCode = 500;
            return Task.CompletedTask;
        });

        Assert.Equal(1, _tracker.RecordedErrors);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseStatusIs503_RecordsServerError()
    {
        var middleware = new ErrorRateTrackingMiddleware(_tracker);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, ctx =>
        {
            ctx.Response.StatusCode = 503;
            return Task.CompletedTask;
        });

        Assert.Equal(1, _tracker.RecordedErrors);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseStatusIs200_DoesNotRecordError()
    {
        var middleware = new ErrorRateTrackingMiddleware(_tracker);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, ctx =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        Assert.Equal(0, _tracker.RecordedErrors);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseStatusIs400_DoesNotRecordError()
    {
        var middleware = new ErrorRateTrackingMiddleware(_tracker);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, ctx =>
        {
            ctx.Response.StatusCode = 400;
            return Task.CompletedTask;
        });

        Assert.Equal(0, _tracker.RecordedErrors);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_RecordsServerError()
    {
        var middleware = new ErrorRateTrackingMiddleware(_tracker);
        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(context, _ => throw new InvalidOperationException("boom")));

        Assert.Equal(0, _tracker.RecordedErrors);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        var middleware = new ErrorRateTrackingMiddleware(_tracker);
        var context = new DefaultHttpContext();
        var nextCalled = false;

        await middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
    }

    private sealed class FakeErrorRateTracker : IErrorRateTracker
    {
        public int RecordedErrors { get; private set; }

        public void RecordServerError()
        {
            RecordedErrors++;
        }

        public ErrorRateSnapshot GetSnapshot() =>
            new(RecordedErrors, TimeSpan.FromMinutes(5), RecordedErrors / 5.0);
    }
}
