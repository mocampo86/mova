using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;
using Mova.Contracts.Reservations;

namespace Mova.IntegrationTests.Reservations;

[Collection("AuthIntegrationTests")]
public sealed class ReservationsControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public ReservationsControllerTests(MovaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateMyReservation_WithValidSlot_ReturnsCreatedReservation()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-user-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1),
            Notes = "Bring a ball"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(court.Id, result.CourtId);
        Assert.Equal("Confirmed", result.Status);
        Assert.Equal("Web", result.Source);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task CreateMyReservation_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{Guid.NewGuid()}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateMyReservation_WithInvalidData_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-invalid-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(-1)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateMyReservation_WithOverlappingSlot_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-overlap-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var firstResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start.AddMinutes(30),
            EndAt = start.AddMinutes(90)
        });

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Create_AsAdmin_ForAnotherUser_ReturnsCreatedReservation()
    {
        var client = _factory.CreateClient();
        var adminSuffix = $"reservation-admin-{Guid.NewGuid()}";
        var adminToken = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var playerSuffix = $"reservation-player-{Guid.NewGuid()}";
        var playerLogin = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-{playerSuffix}" });
        playerLogin.EnsureSuccessStatusCode();
        var playerInfo = await playerLogin.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        Assert.NotNull(playerInfo);

        var start = DateTime.UtcNow.AddDays(2);
        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations", new CreateReservationRequest
        {
            CourtId = court.Id,
            UserId = playerInfo.User.Id,
            StartAt = start,
            EndAt = start.AddHours(1),
            Notes = "Admin booking"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(result);
        Assert.Equal(playerInfo.User.Id, result.UserId);
        Assert.Equal("Confirmed", result.Status);
        Assert.Equal("Admin", result.Source);
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
            Name = $"Reservation Complex {Guid.NewGuid()}",
            Description = "Complex",
            Address = "Address",
            City = "Montevideo",
            PhoneNumber = "+598 99 123 456",
            Email = $"reservation-{Guid.NewGuid()}@test.com"
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
