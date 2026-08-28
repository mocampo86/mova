using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Errors;
using Xunit;

namespace Mova.IntegrationTests.RateLimiting;

public class RateLimitingTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public RateLimitingTests(MovaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ExceedsPermitLimit_Returns429WithStructuredErrorAndRetryAfter()
    {
        var rateLimitedFactory = _factory.WithRateLimits(
            login: new() { PermitLimit = 2, WindowSeconds = 60 });

        using var client = rateLimitedFactory.CreateClient();

        for (var i = 0; i < 2; i++)
        {
            var successResponse = await client.PostAsJsonAsync(
                "/api/v1/auth/google",
                new GoogleLoginRequest { IdToken = $"valid-token-{i}" });
            Assert.Equal(HttpStatusCode.OK, successResponse.StatusCode);
        }

        var limitedResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/google",
            new GoogleLoginRequest { IdToken = "valid-token-3" });

        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        Assert.NotNull(limitedResponse.Headers.RetryAfter);
        Assert.True(limitedResponse.Headers.RetryAfter.Delta?.TotalSeconds > 0);

        var error = await limitedResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("RATE_LIMIT_EXCEEDED", error!.Error.Code);
        Assert.Equal("Too many requests. Please try again later.", error.Error.Message);
        Assert.False(string.IsNullOrWhiteSpace(error.Error.TraceId));
    }

    [Fact]
    public async Task Reservation_ExceedsPermitLimit_Returns429WithStructuredErrorAndRetryAfter()
    {
        var rateLimitedFactory = _factory.WithRateLimits(
            reservation: new() { PermitLimit = 2, WindowSeconds = 60 });

        using var client = rateLimitedFactory.CreateClient();

        var token = await LoginAsync(client, "valid-token-rate-limit");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        for (var i = 0; i < 2; i++)
        {
            var successResponse = await client.PostAsJsonAsync(
                $"/api/v1/complexes/{Guid.NewGuid()}/reservations",
                new { });
            Assert.True(successResponse.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableContent);
        }

        var limitedResponse = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{Guid.NewGuid()}/reservations",
            new { });

        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        Assert.NotNull(limitedResponse.Headers.RetryAfter);
        Assert.True(limitedResponse.Headers.RetryAfter.Delta?.TotalSeconds > 0);

        var error = await limitedResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("RATE_LIMIT_EXCEEDED", error!.Error.Code);
    }

    [Fact]
    public async Task Reservation_DifferentUsers_HaveIndependentQuotas()
    {
        var rateLimitedFactory = _factory.WithRateLimits(
            login: new() { PermitLimit = 10, WindowSeconds = 60 },
            reservation: new() { PermitLimit = 2, WindowSeconds = 60 });

        using var client = rateLimitedFactory.CreateClient();

        var userAToken = await LoginAsync(client, "valid-token-user-a");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAToken);

        for (var i = 0; i < 2; i++)
        {
            var successResponse = await client.PostAsJsonAsync(
                $"/api/v1/complexes/{Guid.NewGuid()}/reservations/me",
                new { });
            Assert.Equal(HttpStatusCode.BadRequest, successResponse.StatusCode);
        }

        var userABlockedResponse = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{Guid.NewGuid()}/reservations/me",
            new { });
        Assert.Equal(HttpStatusCode.TooManyRequests, userABlockedResponse.StatusCode);

        var userBToken = await LoginAsync(client, "valid-token-user-b");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userBToken);

        var userBAllowedResponse = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{Guid.NewGuid()}/reservations/me",
            new { });
        Assert.Equal(HttpStatusCode.BadRequest, userBAllowedResponse.StatusCode);
    }

    [Fact]
    public async Task UnrelatedEndpoint_IsNotAffectedByLoginRateLimit()
    {
        var rateLimitedFactory = _factory.WithRateLimits(
            login: new() { PermitLimit = 2, WindowSeconds = 60 });

        using var client = rateLimitedFactory.CreateClient();

        for (var i = 0; i < 2; i++)
        {
            var successResponse = await client.PostAsJsonAsync(
                "/api/v1/auth/google",
                new GoogleLoginRequest { IdToken = $"valid-token-{i}" });
            Assert.Equal(HttpStatusCode.OK, successResponse.StatusCode);
        }

        var blockedLoginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/google",
            new GoogleLoginRequest { IdToken = "valid-token-3" });
        Assert.Equal(HttpStatusCode.TooManyRequests, blockedLoginResponse.StatusCode);

        var unrelatedResponse = await client.GetAsync("/api/v1/complexes");
        Assert.Equal(HttpStatusCode.OK, unrelatedResponse.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client, string idToken)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/google",
            new GoogleLoginRequest { IdToken = idToken });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return result!.AccessToken;
    }
}
