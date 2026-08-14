using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;
using Mova.Contracts.Reservations;
using Xunit;

namespace Mova.IntegrationTests.Reservations;

[Collection("AuthIntegrationTests")]
public sealed class CancellationPolicyControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public CancellationPolicyControllerTests(MovaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_WithoutConfiguredPolicy_ReturnsDefaultValues()
    {
        var client = _factory.CreateClient();
        var suffix = $"policy-get-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/configuration/cancellation-policy");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CancellationPolicyInfo>();
        Assert.NotNull(result);
        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(24, result.MinimumHours);
        Assert.True(result.AllowUserCancellation);
    }

    [Fact]
    public async Task Update_StoresAndReturnsConfiguredPolicy()
    {
        var client = _factory.CreateClient();
        var suffix = $"policy-update-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/configuration/cancellation-policy",
            new UpdateCancellationPolicyRequest { MinimumHours = 6, AllowUserCancellation = false });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CancellationPolicyInfo>();
        Assert.NotNull(result);
        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(6, result.MinimumHours);
        Assert.False(result.AllowUserCancellation);

        var persistedResponse = await client.GetAsync($"/api/v1/complexes/{complex.Id}/configuration/cancellation-policy");
        persistedResponse.EnsureSuccessStatusCode();
        var persisted = await persistedResponse.Content.ReadFromJsonAsync<CancellationPolicyInfo>();
        Assert.NotNull(persisted);
        Assert.Equal(6, persisted.MinimumHours);
        Assert.False(persisted.AllowUserCancellation);
    }

    [Fact]
    public async Task Update_WithNegativeMinimumHours_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var suffix = $"policy-invalid-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/configuration/cancellation-policy",
            new UpdateCancellationPolicyRequest { MinimumHours = -1, AllowUserCancellation = true });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var suffix = $"policy-unauthorized-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.PutAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/configuration/cancellation-policy",
            new UpdateCancellationPolicyRequest { MinimumHours = 12, AllowUserCancellation = true });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CancelMyReservation_RespectsComplexCancellationPolicy()
    {
        var client = _factory.CreateClient();
        var suffix = $"policy-cancel-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(2);

        await client.PutAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/configuration/cancellation-policy",
            new UpdateCancellationPolicyRequest { MinimumHours = 0, AllowUserCancellation = false });

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var cancelResponse = await client.PatchAsJsonAsync($"/api/v1/users/me/reservations/{created.Id}/cancel", new CancelReservationRequest());

        Assert.Equal(HttpStatusCode.Conflict, cancelResponse.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        return (await LoginWithUserAsync(client, suffix)).AccessToken;
    }

    private static async Task<GoogleLoginResponse> LoginWithUserAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GoogleLoginResponse>())!;
    }

    private static async Task<SportsComplexInfo> CreateComplexAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/complexes", new CreateComplexRequest
        {
            Name = $"Policy Complex {Guid.NewGuid()}",
            Description = "Complex",
            Address = "Address",
            City = "Montevideo",
            PhoneNumber = "+598 99 123 456",
            Email = $"policy-{Guid.NewGuid()}@test.com"
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
            Indoor = true,
            SportIds = []
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CourtInfo>())!;
    }
}
