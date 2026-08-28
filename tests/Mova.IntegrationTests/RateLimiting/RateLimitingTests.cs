using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
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
    public async Task Login_ExceedsPermitLimit_Returns429()
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
    }

    [Fact]
    public async Task Reservation_ExceedsPermitLimit_Returns429()
    {
        var rateLimitedFactory = _factory.WithRateLimits(
            reservation: new() { PermitLimit = 2, WindowSeconds = 60 });

        using var client = rateLimitedFactory.CreateClient();

        var token = await LoginAsync(client);
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
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/google",
            new GoogleLoginRequest { IdToken = "valid-token-rate-limit" });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return result!.AccessToken;
    }
}
