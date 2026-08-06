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

    [Fact]
    public async Task UpdateAvailability_WithValidData_ReturnsUpdatedRules()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/availability", new UpdateCourtAvailabilityRulesRequest
        {
            Rules =
            [
                new CreateCourtAvailabilityRuleRequest
                {
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(12),
                    SlotDurationMinutes = 60,
                    IsActive = true
                }
            ]
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CourtAvailabilityRuleInfo>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(DayOfWeek.Monday, result[0].DayOfWeek);
    }

    [Fact]
    public async Task GetAvailability_AfterCreation_ReturnsRules()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-get-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await client.PutAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/availability", new UpdateCourtAvailabilityRulesRequest
        {
            Rules =
            [
                new CreateCourtAvailabilityRuleRequest
                {
                    DayOfWeek = DayOfWeek.Tuesday,
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(11),
                    SlotDurationMinutes = 60,
                    IsActive = true
                }
            ]
        });

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/availability");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CourtAvailabilityRuleInfo>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(DayOfWeek.Tuesday, result[0].DayOfWeek);
    }

    private static async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GoogleLoginResponse>())!.AccessToken;
    }

    private static async Task<SportsComplexInfo> CreateComplexAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/complexes", new CreateComplexRequest
        {
            Name = $"Availability Complex {Guid.NewGuid()}",
            Description = "Complex",
            Address = "Address",
            City = "Montevideo",
            PhoneNumber = "+598 99 123 456",
            Email = $"availability-{Guid.NewGuid()}@test.com"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SportsComplexInfo>())!;
    }

    private static async Task<CourtInfo> CreateCourtAsync(HttpClient client, Guid complexId)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complexId}/courts", new CreateCourtRequest
        {
            Name = $"Court {Guid.NewGuid()}",
            Description = "Court",
            SurfaceType = "Synthetic",
            Indoor = true
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CourtInfo>())!;
    }
}
