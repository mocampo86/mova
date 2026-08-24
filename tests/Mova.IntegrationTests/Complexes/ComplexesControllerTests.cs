using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mova.Contracts.Auth;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;
using Mova.Contracts.Users;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Helpers;
using Mova.Infrastructure.Data;
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
    public async Task GetActiveList_ExcludesInactiveComplexes()
    {
        var client = _factory.CreateClient();
        var suffix = $"list-admin-{Guid.NewGuid()}";

        var initialToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);

        var activeRequest = CreateValidRequest();
        var activeResponse = await client.PostAsJsonAsync("/api/v1/complexes", activeRequest);
        activeResponse.EnsureSuccessStatusCode();
        var active = await activeResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(active);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var inactiveRequest = CreateValidRequest();
        inactiveRequest.Name = "Inactive Club";
        var inactiveResponse = await client.PostAsJsonAsync("/api/v1/complexes", inactiveRequest);
        inactiveResponse.EnsureSuccessStatusCode();
        var inactive = await inactiveResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(inactive);

        var deactivateResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{inactive.Id}/status", new UpdateComplexStatusRequest { Status = "Inactive" });
        deactivateResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = null;

        var listResponse = await client.GetAsync("/api/v1/complexes");
        listResponse.EnsureSuccessStatusCode();

        var result = await listResponse.Content.ReadFromJsonAsync<PagedResult<SportsComplexInfo>>();
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.Null(item.Status));
        Assert.Contains(result.Items, item => item.Id == active.Id);
        Assert.DoesNotContain(result.Items, item => item.Id == inactive.Id);
    }

    [Fact]
    public async Task GetActiveList_WithBypassParameter_IgnoresItAndReturnsOnlyActive()
    {
        var client = _factory.CreateClient();
        var suffix = $"list-bypass-{Guid.NewGuid()}";

        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", CreateValidRequest());
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var deactivateResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{created.Id}/status", new UpdateComplexStatusRequest { Status = "Inactive" });
        deactivateResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = null;

        var listResponse = await client.GetAsync("/api/v1/complexes?includePending=true&includeInactive=true");
        listResponse.EnsureSuccessStatusCode();

        var result = await listResponse.Content.ReadFromJsonAsync<PagedResult<SportsComplexInfo>>();
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.Null(item.Status));
        Assert.DoesNotContain(result.Items, item => item.Id == created.Id);
    }

    [Fact]
    public async Task GetActiveById_WithPendingComplex_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var suffix = $"get-pending-admin-{Guid.NewGuid()}";

        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var completeResponse = await client.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", new CompleteComplexAdminRequest
        {
            PhoneNumber = "+54 11 1234 5678",
            Name = "Pending Club",
            Description = "A pending club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            Latitude = -34.6m,
            Longitude = -58.3m,
            ComplexPhoneNumber = "+54 11 1234 5678",
            ComplexEmail = "pending@clubpadel.com",
            TimeZoneId = "America/Argentina/Buenos_Aires"
        });
        completeResponse.EnsureSuccessStatusCode();

        var completed = await completeResponse.Content.ReadFromJsonAsync<CompleteComplexAdminResponse>();
        Assert.NotNull(completed);

        client.DefaultRequestHeaders.Authorization = null;

        var getResponse = await client.GetAsync($"/api/v1/complexes/{completed.ComplexId}?includePending=true");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetActiveById_WithActiveComplex_ReturnsComplex()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"get-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", request);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        client.DefaultRequestHeaders.Authorization = null;

        var getResponse = await client.GetAsync($"/api/v1/complexes/{created.Id}");
        getResponse.EnsureSuccessStatusCode();

        var result = await getResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Null(result.Status);
    }

    [Fact]
    public async Task GetActiveById_WithInactiveComplex_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var suffix = $"get-inactive-admin-{Guid.NewGuid()}";

        var initialToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);

        var request = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", request);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var deactivateResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{created.Id}/status", new UpdateComplexStatusRequest { Status = "Inactive" });
        deactivateResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = null;

        var getResponse = await client.GetAsync($"/api/v1/complexes/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetByIdForAdmin_AsComplexAdmin_ReturnsPendingComplex()
    {
        var client = _factory.CreateClient();
        var suffix = $"admin-pending-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var completeResponse = await client.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", new CompleteComplexAdminRequest
        {
            PhoneNumber = "+54 11 1234 5678",
            Name = "Pending Club",
            Description = "A pending club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            Latitude = -34.6m,
            Longitude = -58.3m,
            ComplexPhoneNumber = "+54 11 1234 5678",
            ComplexEmail = "pending@clubpadel.com",
            TimeZoneId = "America/Argentina/Buenos_Aires"
        });
        completeResponse.EnsureSuccessStatusCode();

        var completed = await completeResponse.Content.ReadFromJsonAsync<CompleteComplexAdminResponse>();
        Assert.NotNull(completed);
        Assert.NotEqual(Guid.Empty, completed.ComplexId);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync($"/api/v1/complexes/{completed.ComplexId}/admin");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(result);
        Assert.Equal(completed.ComplexId, result.Id);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task GetByIdForAdmin_AsNonAdmin_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var adminSuffix = $"admin-owner-{Guid.NewGuid()}";
        var otherSuffix = $"admin-other-{Guid.NewGuid()}";

        var adminToken = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", CreateValidRequest());
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var otherToken = await LoginAsync(client, otherSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        var response = await client.GetAsync($"/api/v1/complexes/{created.Id}/admin");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
            TimeZoneId = "America/Argentina/Buenos_Aires"
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
        Assert.Equal("Active", result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task Update_ImmediatelyAfterCreation_WithSameToken_ReturnsUpdatedComplex()
    {
        var client = _factory.CreateClient();
        var suffix = $"immediate-admin-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var updateRequest = new UpdateComplexRequest
        {
            Name = "Updated Club",
            Description = "Updated description",
            Address = "Updated address",
            City = "Updated city",
            PhoneNumber = "+54 11 9999 9999",
            Email = "updated@clubpadel.com",
            TimeZoneId = "America/Argentina/Buenos_Aires"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/complexes/{created.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var result = await updateResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Updated Club", result.Name);
    }

    [Fact]
    public async Task Update_AsComplexAdmin_OnPendingComplex_UpdatesAndPreservesPendingStatus()
    {
        var client = _factory.CreateClient();
        var suffix = $"update-pending-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var completeResponse = await client.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", new CompleteComplexAdminRequest
        {
            PhoneNumber = "+54 11 1234 5678",
            Name = "Pending Club",
            Description = "A pending club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            Latitude = -34.6m,
            Longitude = -58.3m,
            ComplexPhoneNumber = "+54 11 1234 5678",
            ComplexEmail = "pending@clubpadel.com",
            TimeZoneId = "America/Argentina/Buenos_Aires"
        });
        completeResponse.EnsureSuccessStatusCode();

        var completed = await completeResponse.Content.ReadFromJsonAsync<CompleteComplexAdminResponse>();
        Assert.NotNull(completed);
        Assert.NotEqual(Guid.Empty, completed.ComplexId);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var updateRequest = new UpdateComplexRequest
        {
            Name = "Updated Pending Club",
            Description = "Updated description",
            Address = "Updated address",
            City = "Updated city",
            PhoneNumber = "+54 11 9999 9999",
            Email = "updated@clubpadel.com",
            TimeZoneId = "America/Argentina/Buenos_Aires"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/complexes/{completed.ComplexId}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var result = await updateResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(result);
        Assert.Equal(completed.ComplexId, result.Id);
        Assert.Equal("Updated Pending Club", result.Name);
        Assert.Equal("Pending", result.Status);
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
            TimeZoneId = "America/Argentina/Buenos_Aires"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/complexes/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsComplexAdmin_TogglesComplexStatus()
    {
        var client = _factory.CreateClient();
        var suffix = $"status-admin-{Guid.NewGuid()}";

        var initialToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);

        var createRequest = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var deactivateResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{created.Id}/status", new UpdateComplexStatusRequest { Status = "Inactive" });
        deactivateResponse.EnsureSuccessStatusCode();

        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(deactivated);
        Assert.Equal("Inactive", deactivated.Status);
        Assert.NotNull(deactivated.UpdatedAt);

        var activateResponse = await client.PatchAsJsonAsync($"/api/v1/complexes/{created.Id}/status", new UpdateComplexStatusRequest { Status = "Active" });
        activateResponse.EnsureSuccessStatusCode();

        var activated = await activateResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(activated);
        Assert.Equal("Active", activated.Status);
    }

    [Fact]
    public async Task UpdateStatus_AsNonAdmin_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var adminSuffix = $"status-admin-{Guid.NewGuid()}";
        var otherSuffix = $"status-other-{Guid.NewGuid()}";

        var initialToken = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);

        var createRequest = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var otherToken = await LoginAsync(client, otherSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{created.Id}/status", new UpdateComplexStatusRequest { Status = "Inactive" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var suffix = $"status-invalid-{Guid.NewGuid()}";

        var initialToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialToken);

        var createRequest = CreateValidRequest();
        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var adminToken = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.PatchAsJsonAsync($"/api/v1/complexes/{created.Id}/status", new UpdateComplexStatusRequest { Status = "Unknown" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_AsComplexAdmin_ReturnsAggregatedMetrics()
    {
        var client = _factory.CreateClient();
        var suffix = $"dashboard-admin-{Guid.NewGuid()}";
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", CreateValidRequest());
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        await SeedDashboardDataAsync(created.Id, suffix);

        var response = await client.GetAsync($"/api/v1/complexes/{created.Id}/dashboard");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ComplexDashboardInfo>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Complex.Id);
        Assert.Equal(created.Name, result.Complex.Name);
        Assert.Equal("Active", result.Complex.Status);
        Assert.Equal(1, result.Courts.Active);
        Assert.Equal(1, result.Courts.Inactive);
        Assert.Equal(2, result.ReservationsToday.Confirmed);
        Assert.Equal(1, result.ReservationsToday.Cancelled);
        Assert.Equal(1, result.ReservationsToday.Completed);
        Assert.Equal(2, result.BlockedUsers);
    }

    [Fact]
    public async Task GetDashboard_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/complexes/{Guid.NewGuid()}/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_AsAdminOfDifferentComplex_ReturnsForbidden()
    {
        var client = _factory.CreateClient();

        var adminSuffix = $"dashboard-owner-{Guid.NewGuid()}";
        var ownerToken = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var createResponse = await client.PostAsJsonAsync("/api/v1/complexes", CreateValidRequest());
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(created);

        var otherSuffix = $"dashboard-other-{Guid.NewGuid()}";
        var otherToken = await LoginAsync(client, otherSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        var response = await client.GetAsync($"/api/v1/complexes/{created.Id}/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedDashboardDataAsync(Guid complexId, string suffix)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var admin = await context.Users.FirstAsync(u => u.GoogleSubjectId == $"sub-{suffix}");

        var activeCourt = Court.Create(complexId, "Active Court", "Court", "Synthetic", true);
        var inactiveCourt = Court.Create(complexId, "Inactive Court", "Court", "Grass", false);
        inactiveCourt.Deactivate();
        context.Courts.Add(activeCourt);
        context.Courts.Add(inactiveCourt);

        var player = User.CreateFromGoogle(Guid.NewGuid(), $"player-{suffix}", $"player-{suffix}@test.com", $"Player {suffix}");
        context.Users.Add(player);

        var complex = await context.SportsComplexes.FindAsync(complexId);
        var timeZone = TimeZoneConverter.GetTimeZone(complex!.TimeZoneId!);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var start = TimeZoneConverter.GetDayStartUtc(today, timeZone);

        var confirmed1 = Reservation.Create(complexId, activeCourt.Id, player.Id, start.AddHours(10), start.AddHours(11), ReservationSource.Web);
        confirmed1.Confirm();
        var confirmed2 = Reservation.Create(complexId, activeCourt.Id, player.Id, start.AddHours(12), start.AddHours(13), ReservationSource.Web);
        confirmed2.Confirm();
        var cancelled = Reservation.Create(complexId, activeCourt.Id, player.Id, start.AddHours(14), start.AddHours(15), ReservationSource.Web);
        cancelled.Cancel(player.Id);
        var completed = Reservation.Create(complexId, activeCourt.Id, player.Id, start.AddHours(16), start.AddHours(17), ReservationSource.Web);
        completed.Confirm();
        context.Entry(completed).Property(r => r.Status).CurrentValue = ReservationStatus.Completed;

        context.Reservations.AddRange(confirmed1, confirmed2, cancelled, completed);

        var blocked1 = BlockedUser.Create(complexId, Guid.NewGuid(), admin.Id);
        var blocked2 = BlockedUser.Create(complexId, Guid.NewGuid(), admin.Id);
        context.BlockedUsers.AddRange(blocked1, blocked2);

        await context.SaveChangesAsync();
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
            Email = "contact@clubpadel.com",
            TimeZoneId = "America/Argentina/Buenos_Aires"
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
