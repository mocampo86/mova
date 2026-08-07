using System.Net;
using System.Net.Http.Headers;
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

    [Fact]
    public async Task CompleteComplexAdmin_WithValidData_CreatesPendingComplexAndReturnsToken()
    {
        var suffix = $"complex-admin-{Guid.NewGuid()}";
        var token = await LoginAsync(suffix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CompleteComplexAdminRequest
        {
            PhoneNumber = "+54 11 1234 5678",
            Name = "Club Padel",
            Description = "A premium padel club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            Latitude = -34.6m,
            Longitude = -58.3m,
            ComplexPhoneNumber = "+54 11 1234 5678",
            ComplexEmail = "contact@clubpadel.com"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CompleteComplexAdminResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.NotEqual(Guid.Empty, result.ComplexId);
        Assert.False(result.RequiresProfileCompletion);
        Assert.Equal("+54 11 1234 5678", result.User.PhoneNumber);
    }

    [Fact]
    public async Task CompleteComplexAdmin_WithInvalidData_ReturnsBadRequest()
    {
        var suffix = $"complex-admin-invalid-{Guid.NewGuid()}";
        var token = await LoginAsync(suffix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CompleteComplexAdminRequest { Name = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteComplexAdmin_WithoutAuthorization_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var request = new CompleteComplexAdminRequest
        {
            PhoneNumber = "+54 11 1234 5678",
            Name = "Club Padel",
            Description = "A premium padel club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            ComplexPhoneNumber = "+54 11 1234 5678",
            ComplexEmail = "contact@clubpadel.com"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> LoginAsync(string suffix)
    {
        var request = new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/google", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return result!.AccessToken;
    }
}
