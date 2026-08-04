using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Users;
using Mova.IntegrationTests.Authentication;
using Xunit;

namespace Mova.IntegrationTests.Users;

[Collection("AuthIntegrationTests")]
public class UsersControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public UsersControllerTests(MovaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteProfile_WithValidPhoneNumber_UpdatesAndReturnsUserInfo()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CompleteProfileRequest { PhoneNumber = "+54 11 1234 5678" };
        var response = await client.PatchAsJsonAsync("/api/v1/users/me", request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserInfo>();

        Assert.NotNull(result);
        Assert.Equal("+54 11 1234 5678", result.PhoneNumber);
        Assert.False(result.PhoneVerified);
    }

    [Fact]
    public async Task CompleteProfile_WithInvalidPhoneNumber_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CompleteProfileRequest { PhoneNumber = "invalid" };
        var response = await client.PatchAsJsonAsync("/api/v1/users/me", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteProfile_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var request = new CompleteProfileRequest { PhoneNumber = "+54 11 1234 5678" };

        var response = await client.PatchAsJsonAsync("/api/v1/users/me", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var idToken = $"valid-token-{Guid.NewGuid()}";
        var loginRequest = new GoogleLoginRequest { IdToken = idToken };
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", loginRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return result!.AccessToken;
    }
}
