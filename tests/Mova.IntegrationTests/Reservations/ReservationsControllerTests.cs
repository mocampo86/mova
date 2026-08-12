using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

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

    [Fact]
    public async Task GetMyUpcoming_ReturnsOnlyAuthenticatedUsersFutureActiveReservations()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-upcoming-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var now = DateTime.UtcNow;
        var soonerStart = now.AddDays(1);
        var laterStart = now.AddDays(2);
        Guid soonerId;
        Guid laterId;

        var otherUserLogin = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-other-{Guid.NewGuid()}" });
        otherUserLogin.EnsureSuccessStatusCode();
        var otherUser = await otherUserLogin.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        Assert.NotNull(otherUser);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

            var past = Reservation.Create(complex.Id, court.Id, login.User.Id, now.AddDays(-1), now.AddDays(-1).AddHours(1), ReservationSource.Web);
            past.Confirm();
            var sooner = Reservation.Create(complex.Id, court.Id, login.User.Id, soonerStart, soonerStart.AddHours(1), ReservationSource.Web);
            sooner.Confirm();
            var later = Reservation.Create(complex.Id, court.Id, login.User.Id, laterStart, laterStart.AddHours(1), ReservationSource.Web);
            later.Confirm();
            soonerId = sooner.Id;
            laterId = later.Id;
            var otherUsers = Reservation.Create(complex.Id, court.Id, otherUser.User.Id, soonerStart.AddHours(2), soonerStart.AddHours(3), ReservationSource.Web);
            otherUsers.Confirm();
            var cancelled = Reservation.Create(complex.Id, court.Id, login.User.Id, soonerStart.AddHours(4), soonerStart.AddHours(5), ReservationSource.Web);
            cancelled.Cancel("No longer needed");

            await context.Reservations.AddRangeAsync(past, later, sooner, otherUsers, cancelled);
            await context.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/v1/users/me/reservations?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal([soonerId, laterId], result.Items.Select(r => r.Id));
        Assert.All(result.Items, reservation => Assert.Equal(login.User.Id, reservation.UserId));
    }

    [Fact]
    public async Task GetMyHistory_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/users/me/reservations/history?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyHistory_WithInvalidPageSize_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-history-invalid-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/users/me/reservations/history?page=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMyHistory_ReturnsOnlyAuthenticatedUsersPastAndCompletedOrCancelledReservations()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-history-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var now = DateTime.UtcNow;
        var futureStart = now.AddDays(1);
        Guid pastId;
        Guid completedId;
        Guid cancelledId;

        var otherUserLogin = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-other-{Guid.NewGuid()}" });
        otherUserLogin.EnsureSuccessStatusCode();
        var otherUser = await otherUserLogin.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        Assert.NotNull(otherUser);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

            var past = Reservation.Create(complex.Id, court.Id, login.User.Id, now.AddDays(-2), now.AddDays(-2).AddHours(1), ReservationSource.Web);
            past.Confirm();
            pastId = past.Id;

            var completed = Reservation.Create(complex.Id, court.Id, login.User.Id, futureStart, futureStart.AddHours(1), ReservationSource.Web);
            completed.Confirm();
            completed.MarkCompleted();
            completedId = completed.Id;

            var cancelled = Reservation.Create(complex.Id, court.Id, login.User.Id, futureStart.AddHours(2), futureStart.AddHours(3), ReservationSource.Web);
            cancelled.Confirm();
            cancelled.Cancel("No longer needed");
            cancelledId = cancelled.Id;

            var upcoming = Reservation.Create(complex.Id, court.Id, login.User.Id, futureStart.AddHours(4), futureStart.AddHours(5), ReservationSource.Web);
            upcoming.Confirm();

            var otherUsers = Reservation.Create(complex.Id, court.Id, otherUser.User.Id, now.AddDays(-1), now.AddDays(-1).AddHours(1), ReservationSource.Web);
            otherUsers.Confirm();

            await context.Reservations.AddRangeAsync(past, completed, cancelled, upcoming, otherUsers);
            await context.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/v1/users/me/reservations/history?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal([cancelledId, completedId, pastId], result.Items.Select(r => r.Id));
        Assert.All(result.Items, reservation => Assert.Equal(login.User.Id, reservation.UserId));
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
