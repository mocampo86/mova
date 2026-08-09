using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Common;
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
    public async Task UpdateStatus_AsAdmin_DeactivatesAndActivatesCourt()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"status-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var deactivateResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/status", new UpdateCourtStatusRequest { Status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<CourtInfo>();
        Assert.NotNull(deactivated);
        Assert.Equal("Inactive", deactivated.Status);

        var activateResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/status", new UpdateCourtStatusRequest { Status = "Active" });
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        var activated = await activateResponse.Content.ReadFromJsonAsync<CourtInfo>();
        Assert.NotNull(activated);
        Assert.Equal("Active", activated.Status);
    }

    [Fact]
    public async Task UpdateStatus_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{Guid.NewGuid()}/courts/{Guid.NewGuid()}/status", new UpdateCourtStatusRequest { Status = "Inactive" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"status-invalid-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/status", new UpdateCourtStatusRequest { Status = "Invalid" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveList_ExcludesInactiveCourts()
    {
        var client = _factory.CreateClient();
        var suffix = $"list-admin-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var activeCourt = await CreateCourtAsync(client, complex.Id);
        var inactiveCourt = await CreateCourtAsync(client, complex.Id);
        await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{inactiveCourt.Id}/status", new UpdateCourtStatusRequest { Status = "Inactive" });

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/courts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CourtInfo>>();
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.Equal("Active", item.Status));
        Assert.Contains(result.Items, item => item.Id == activeCourt.Id);
        Assert.DoesNotContain(result.Items, item => item.Id == inactiveCourt.Id);
    }

    [Fact]
    public async Task GetActiveById_WithActiveCourt_ReturnsCourt()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"get-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/courts/{court.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CourtInfo>();
        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task GetActiveById_WithInactiveCourt_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"get-inactive-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/status", new UpdateCourtStatusRequest { Status = "Inactive" });

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/courts/{court.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_AsAdmin_WithInactiveCourt_ReturnsCourt()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"get-admin-inactive-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/status", new UpdateCourtStatusRequest { Status = "Inactive" });

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CourtInfo>();
        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
        Assert.Equal("Inactive", result.Status);
    }

    [Fact]
    public async Task GetById_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/complexes/{Guid.NewGuid()}/courts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedCourt()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"update-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}", new UpdateCourtRequest
        {
            Name = "Updated Court",
            Description = "Updated description",
            SurfaceType = "Grass",
            Indoor = true,
            SportIds = []
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CourtInfo>();
        Assert.NotNull(result);
        Assert.Equal(court.Id, result.Id);
        Assert.Equal("Updated Court", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("Grass", result.SurfaceType);
        Assert.True(result.Indoor);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task Update_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{Guid.NewGuid()}/courts/{Guid.NewGuid()}", new UpdateCourtRequest
        {
            Name = "Court",
            Description = "Description",
            SurfaceType = "Synthetic"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithInvalidData_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"update-invalid-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}", new UpdateCourtRequest
        {
            Name = "",
            Description = "",
            SurfaceType = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithDuplicateName_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"update-duplicate-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        var firstCourt = await CreateCourtAsync(client, complex.Id);
        var secondCourt = await CreateCourtAsync(client, complex.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complex.Id}/courts/{firstCourt.Id}", new UpdateCourtRequest
        {
            Name = secondCourt.Name,
            Description = "Updated description",
            SurfaceType = "Synthetic"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
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
