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
            cancelled.Cancel(login.User.Id, false, "No longer needed");

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
            cancelled.Cancel(login.User.Id, false, "No longer needed");
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

    [Fact]
    public async Task CancelMyReservation_WithinPolicyWindow_ReturnsCancelledByUser()
    {
        var client = _factory.CreateClient();
        var suffix = $"cancel-user-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(2);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var cancelResponse = await client.PatchAsJsonAsync($"/api/v1/users/me/reservations/{created.Id}/cancel", new CancelReservationRequest { Reason = "Changed plans" });

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var result = await cancelResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(result);
        Assert.Equal("CancelledByUser", result.Status);
        Assert.Equal("Changed plans", result.CancellationReason);
        Assert.Equal(login.User.Id, result.CancelledByUserId);
    }

    [Fact]
    public async Task CancelMyReservation_AfterDeadline_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var suffix = $"cancel-late-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddHours(1);

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

    [Fact]
    public async Task Cancel_AsAdmin_ReturnsCancelledByAdmin()
    {
        var client = _factory.CreateClient();
        var suffix = $"cancel-admin-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var cancelResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/cancel", new CancelReservationRequest { Reason = "No show" });

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var result = await cancelResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(result);
        Assert.Equal("CancelledByAdmin", result.Status);
        Assert.Equal("No show", result.CancellationReason);
    }

    [Fact]
    public async Task Cancel_AsAdmin_ForCompletedReservation_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var suffix = $"cancel-admin-completed-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var completeResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/status", new UpdateReservationStatusRequest { Status = "Completed" });
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var cancelResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/cancel", new CancelReservationRequest { Reason = "No show" });

        Assert.Equal(HttpStatusCode.Conflict, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsAdmin_ToCompleted_ReturnsCompletedReservation()
    {
        var client = _factory.CreateClient();
        var suffix = $"update-status-completed-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/status", new UpdateReservationStatusRequest { Status = "Completed" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public async Task UpdateStatus_AsAdmin_ToNoShow_ReturnsNoShowReservation()
    {
        var client = _factory.CreateClient();
        var suffix = $"update-status-noshow-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/status", new UpdateReservationStatusRequest { Status = "NoShow" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(result);
        Assert.Equal("NoShow", result.Status);
    }

    [Fact]
    public async Task UpdateStatus_AsAdmin_WithInvalidStatus_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var suffix = $"update-status-invalid-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/status", new UpdateReservationStatusRequest { Status = "Confirmed" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{Guid.NewGuid()}/reservations/{Guid.NewGuid()}/status", new UpdateReservationStatusRequest { Status = "Completed" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsAdmin_ForDifferentComplex_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var suffixA = $"update-status-owner-{Guid.NewGuid()}";
        var tokenA = await LoginAsync(client, suffixA);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var complexA = await CreateComplexAsync(client);
        var courtA = await CreateCourtAsync(client, complexA.Id);

        var start = DateTime.UtcNow.AddDays(1);
        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complexA.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = courtA.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var suffixB = $"update-status-intruder-{Guid.NewGuid()}";
        var tokenB = await LoginAsync(client, suffixB);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        await CreateComplexAsync(client);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{complexA.Id}/reservations/{created.Id}/status", new UpdateReservationStatusRequest { Status = "Completed" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsAdmin_ForCancelledReservation_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var suffix = $"update-status-cancelled-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var start = DateTime.UtcNow.AddDays(1);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(created);

        var cancelResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/cancel", new CancelReservationRequest { Reason = "No longer needed" });
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{created.Id}/status", new UpdateReservationStatusRequest { Status = "Completed" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetList_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/complexes/{Guid.NewGuid()}/reservations?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetList_AsComplexAdmin_ReturnsAllReservationsForComplex()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-list-admin-{Guid.NewGuid()}";
        var initialToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var playerLogin = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-player-{Guid.NewGuid()}" });
        playerLogin.EnsureSuccessStatusCode();
        var playerInfo = await playerLogin.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        Assert.NotNull(playerInfo);

        var start1 = DateTime.UtcNow.AddDays(1);
        var firstResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start1,
            EndAt = start1.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        var first = await firstResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(first);

        var start2 = DateTime.UtcNow.AddDays(2);
        var secondResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations", new CreateReservationRequest
        {
            CourtId = court.Id,
            UserId = playerInfo.User.Id,
            StartAt = start2,
            EndAt = start2.AddHours(1),
            Notes = "Manual admin booking"
        });
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        var second = await secondResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(second);

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, r => r.Id == first.Id);
        Assert.Contains(result.Items, r => r.Id == second.Id);
        Assert.All(result.Items, r => Assert.Equal(complex.Id, r.SportsComplexId));
        Assert.All(result.Items, r => Assert.NotEmpty(r.CourtName));
        Assert.All(result.Items, r => Assert.NotEmpty(r.UserName));
    }

    [Fact]
    public async Task GetList_AsAdminOfDifferentComplex_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var suffixA = $"reservation-list-owner-{Guid.NewGuid()}";
        var tokenA = await LoginAsync(client, suffixA);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var complexA = await CreateComplexAsync(client);

        var adminTokenA = await LoginAsync(client, suffixA);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenA);
        await CreateCourtAsync(client, complexA.Id);

        var suffixB = $"reservation-list-intruder-{Guid.NewGuid()}";
        var tokenB = await LoginAsync(client, suffixB);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var complexB = await CreateComplexAsync(client);

        var adminTokenB = await LoginAsync(client, suffixB);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminTokenB);

        var response = await client.GetAsync($"/api/v1/complexes/{complexA.Id}/reservations?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetList_WithCourtFilter_ReturnsFilteredReservations()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-list-court-filter-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var courtA = await CreateCourtAsync(client, complex.Id);
        var courtB = await CreateCourtAsync(client, complex.Id);

        var start = DateTime.UtcNow.AddDays(1);
        var responseA = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = courtA.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, responseA.StatusCode);
        var createdA = await responseA.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(createdA);

        var responseB = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = courtB.Id,
            StartAt = start.AddHours(2),
            EndAt = start.AddHours(3)
        });
        Assert.Equal(HttpStatusCode.Created, responseB.StatusCode);
        var createdB = await responseB.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(createdB);

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations?page=1&pageSize=10&courtId={courtA.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(createdA.Id, result.Items[0].Id);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task GetList_WithStatusFilter_ReturnsFilteredReservations()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-list-status-filter-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var court = await CreateCourtAsync(client, complex.Id);

        var start = DateTime.UtcNow.AddDays(1);
        var activeResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = start,
            EndAt = start.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, activeResponse.StatusCode);
        var active = await activeResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(active);

        var cancelResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/{active.Id}/cancel", new CancelReservationRequest { Reason = "No longer needed" });
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(cancelled);
        Assert.Equal("CancelledByAdmin", cancelled.Status);

        var activeListResponse = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations?page=1&pageSize=10&status=Confirmed");
        Assert.Equal(HttpStatusCode.OK, activeListResponse.StatusCode);

        var activeResult = await activeListResponse.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(activeResult);
        Assert.Empty(activeResult.Items);
        Assert.Equal(0, activeResult.TotalItems);

        var cancelledListResponse = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations?page=1&pageSize=10&status=CancelledByAdmin");
        Assert.Equal(HttpStatusCode.OK, cancelledListResponse.StatusCode);

        var cancelledResult = await cancelledListResponse.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(cancelledResult);
        Assert.Single(cancelledResult.Items);
        Assert.Equal(active.Id, cancelledResult.Items[0].Id);
        Assert.Equal(1, cancelledResult.TotalItems);
    }

    [Fact]
    public async Task GetList_WithPagination_ReturnsPagedResult()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-list-pagination-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var court = await CreateCourtAsync(client, complex.Id);

        for (var i = 0; i < 3; i++)
        {
            var start = DateTime.UtcNow.AddDays(i + 1);
            var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
            {
                CourtId = court.Id,
                StartAt = start,
                EndAt = start.AddHours(1)
            });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalPages);

        var secondPageResponse = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations?page=2&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);

        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(secondPage);
        Assert.Single(secondPage.Items);
        Assert.Equal(2, secondPage.Page);
        Assert.Equal(3, secondPage.TotalItems);
    }

    [Fact]
    public async Task GetList_WithDateFilter_ReturnsReservationsForRequestedDay()
    {
        var client = _factory.CreateClient();
        var suffix = $"reservation-list-date-filter-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var court = await CreateCourtAsync(client, complex.Id);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 10, 0, 0, DateTimeKind.Utc);
        var tomorrowStart = todayStart.AddDays(1);

        var todayResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = todayStart,
            EndAt = todayStart.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, todayResponse.StatusCode);

        var tomorrowResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = tomorrowStart,
            EndAt = tomorrowStart.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.Created, tomorrowResponse.StatusCode);

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations?page=1&pageSize=10&date={today:yyyy-MM-dd}&utcOffsetMinutes=0");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(today, DateOnly.FromDateTime(result.Items[0].StartAt));
    }

    [Fact]
    public async Task CreateMyRecurringReservation_WithValidPeriod_CreatesSeriesAndOccurrences()
    {
        var client = _factory.CreateClient();
        var suffix = $"recurring-create-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/recurring-reservations/me", new CreateRecurringReservationRequest
        {
            CourtId = court.Id,
            DayOfWeek = (int)DayOfWeek.Monday,
            StartTime = new TimeOnly(14, 0),
            DurationMinutes = 60,
            StartDate = new DateOnly(2026, 8, 10),
            EndDate = new DateOnly(2026, 8, 31),
            Notes = "Weekly match"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecurringReservationInfo>();
        Assert.NotNull(result);
        Assert.Equal("Active", result.Status);
        Assert.Equal(4, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence =>
        {
            Assert.Equal("Confirmed", occurrence.Status);
            Assert.Equal("Recurring", occurrence.Source);
            Assert.Equal(result.Id, occurrence.RecurringReservationId);
        });

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
        var persistedCount = context.Reservations.Count(r => r.RecurringReservationId == result.Id);
        Assert.Equal(4, persistedCount);
    }

    [Fact]
    public async Task CreateMyRecurringReservation_WithOverlappingExistingOccurrence_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var suffix = $"recurring-conflict-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var singleResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = new DateTime(2026, 9, 7, 14, 30, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 9, 7, 15, 30, 0, DateTimeKind.Utc)
        });
        Assert.Equal(HttpStatusCode.Created, singleResponse.StatusCode);

        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/recurring-reservations/me", new CreateRecurringReservationRequest
        {
            CourtId = court.Id,
            DayOfWeek = (int)DayOfWeek.Monday,
            StartTime = new TimeOnly(14, 0),
            DurationMinutes = 60,
            StartDate = new DateOnly(2026, 9, 7),
            EndDate = new DateOnly(2026, 9, 21)
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CancelMyRecurringReservation_CancelsSeriesAndFutureOccurrences()
    {
        var client = _factory.CreateClient();
        var suffix = $"recurring-cancel-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/recurring-reservations/me", new CreateRecurringReservationRequest
        {
            CourtId = court.Id,
            DayOfWeek = (int)DayOfWeek.Monday,
            StartTime = new TimeOnly(14, 0),
            DurationMinutes = 60,
            StartDate = new DateOnly(2026, 9, 14),
            EndDate = new DateOnly(2026, 9, 28)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<RecurringReservationInfo>();
        Assert.NotNull(created);

        var cancelResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/recurring-reservations/{created.Id}/cancel", new CancelReservationRequest
        {
            Reason = "Season changed"
        });

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var result = await cancelResponse.Content.ReadFromJsonAsync<RecurringReservationInfo>();
        Assert.NotNull(result);
        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(3, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence => Assert.Equal("CancelledByUser", occurrence.Status));
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
