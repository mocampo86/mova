using System.Net;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Xunit;

namespace Mova.IntegrationTests.Authentication;

[CollectionDefinition("AuthIntegrationTests", DisableParallelization = true)]
public class AuthIntegrationTestsCollection
{
}

[Collection("AuthIntegrationTests")]
public class AuthControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(MovaWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GoogleLogin_WithNewUser_ReturnsAccessTokenAndRequiresProfileCompletion()
    {
        var request = new GoogleLoginRequest { IdToken = $"valid-token-{Guid.NewGuid()}" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();

        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.True(result.RequiresProfileCompletion);
        Assert.NotEqual(Guid.Empty, result.User.Id);
        Assert.False(string.IsNullOrWhiteSpace(result.User.FullName));
    }

    [Fact]
    public async Task GoogleLogin_WithExistingUser_ReturnsAccessTokenAndUserDetails()
    {
        var idToken = $"valid-token-{Guid.NewGuid()}";
        var firstRequest = new GoogleLoginRequest { IdToken = idToken };
        await _client.PostAsJsonAsync("/api/v1/auth/google", firstRequest);

        var secondRequest = new GoogleLoginRequest { IdToken = idToken };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", secondRequest);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.True(result.RequiresProfileCompletion);
    }

    [Fact]
    public async Task GoogleLogin_WithInvalidToken_ReturnsUnauthorized()
    {
        var request = new GoogleLoginRequest { IdToken = "invalid-token" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GoogleLogin_WithMissingIdToken_ReturnsBadRequest()
    {
        var request = new GoogleLoginRequest { IdToken = string.Empty };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
