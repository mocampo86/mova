using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;

namespace Mova.IntegrationTests.Courts;

[Collection("AuthIntegrationTests")]
public sealed class CourtsControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public CourtsControllerTests(MovaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedCourt()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"court-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complexResponse = await client.PostAsJsonAsync("/api/v1/complexes", new CreateComplexRequest
        {
            Name = "Court Complex", Description = "Complex", Address = "Address", City = "Montevideo",
            PhoneNumber = "+598 99 123 456", Email = $"court-{Guid.NewGuid()}@test.com"
        });
        var complex = await complexResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(complex);

        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts", new CreateCourtRequest
        {
            Name = "Court 1", Description = "Indoor court", SurfaceType = "Synthetic", Indoor = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CourtInfo>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal("Court 1", result.Name);
        Assert.Equal("Active", result.Status);
        Assert.True(result.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task Create_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{Guid.NewGuid()}/courts", new CreateCourtRequest
        {
            Name = "Court 1", Description = "Description", SurfaceType = "Synthetic"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GoogleLoginResponse>())!.AccessToken;
    }
}
