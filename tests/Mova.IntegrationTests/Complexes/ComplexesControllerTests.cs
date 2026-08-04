using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Users;
using Mova.IntegrationTests.Authentication;
using Xunit;

namespace Mova.IntegrationTests.Complexes;

[Collection("AuthIntegrationTests")]
public class ComplexesControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public ComplexesControllerTests(MovaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedComplex()
    {
        var client = _factory.CreateClient();
        var suffix = $"complex-creator-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = CreateValidRequest();
        var response = await client.PostAsJsonAsync("/api/v1/complexes", request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.Address, result.Address);
        Assert.Equal(request.City, result.City);
        Assert.Equal(request.Latitude, result.Latitude);
        Assert.Equal(request.Longitude, result.Longitude);
        Assert.Equal(request.PhoneNumber, result.PhoneNumber);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal("Active", result.Status);
        Assert.True(result.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"invalid-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateComplexRequest { Name = "" };
        var response = await client.PostAsJsonAsync("/api/v1/complexes", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var request = CreateValidRequest();

        var response = await client.PostAsJsonAsync("/api/v1/complexes", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_AsComplexAdmin_ReturnsUpdatedComplex()
    {
        var client = _factory.CreateClient();
        var suffix = $"complex-admin-{Guid.NewGuid()}";
        var initialToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);

        var createRequest = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var updatedToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", updatedToken);

        var updateRequest = new UpdateComplexRequest
        {
            Name = "Updated Club",
            Description = "Updated description",
            Address = "Updated address",
            City = "Updated city",
            Latitude = -33m,
            Longitude = -60m,
            PhoneNumber = "+54 11 9999 9999",
            Email = "updated@clubpadel.com",
            Status = "Inactive"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/complexes/{created.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var result = await updateResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Updated Club", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("Updated address", result.Address);
        Assert.Equal("Updated city", result.City);
        Assert.Equal(-33m, result.Latitude);
        Assert.Equal(-60m, result.Longitude);
        Assert.Equal("+54 11 9999 9999", result.PhoneNumber);
        Assert.Equal("updated@clubpadel.com", result.Email);
        Assert.Equal("Inactive", result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task Update_AsNonAdmin_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var adminSuffix = $"admin-{Guid.NewGuid()}";
        var otherSuffix = $"other-{Guid.NewGuid()}";

        var adminToken = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createRequest = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var otherToken = await LoginAsync(client, otherSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        var updateRequest = new UpdateComplexRequest
        {
            Name = "Updated Club",
            Description = "Updated description",
            Address = "Updated address",
            City = "Updated city",
            PhoneNumber = "+54 11 9999 9999",
            Email = "updated@clubpadel.com",
            Status = "Active"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/complexes/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }

    private static CreateComplexRequest CreateValidRequest() =>
        new()
        {
            Name = "Club Padel",
            Description = "A premium padel club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            Latitude = -34.6m,
            Longitude = -58.3m,
            PhoneNumber = "+54 11 1234 5678",
            Email = "contact@clubpadel.com"
        };

    private static async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        var request = new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return result!.AccessToken;
    }
}
