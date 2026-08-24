using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Common;
using Mova.Contracts.Courts;
using Mova.Contracts.Reservations;
using Mova.Contracts.Users;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;
using Mova.IntegrationTests.Authentication;
using Xunit;

namespace Mova.IntegrationTests.Users;

[Collection("AuthIntegrationTests")]
public class UsersControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public UsersControllerTests(MovaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteProfile_WithValidPhoneNumber_UpdatesAndReturnsUserInfo()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CompleteProfileRequest { PhoneNumber = "+54 11 1234 5678" };
        var response = await client.PatchAsJsonAsync("/api/v1/users/me", request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserInfo>();

        Assert.NotNull(result);
        Assert.Equal("+54 11 1234 5678", result.PhoneNumber);
        Assert.False(result.PhoneVerified);
    }

    [Fact]
    public async Task CompleteProfile_WithInvalidPhoneNumber_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CompleteProfileRequest { PhoneNumber = "invalid" };
        var response = await client.PatchAsJsonAsync("/api/v1/users/me", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteProfile_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var request = new CompleteProfileRequest { PhoneNumber = "+54 11 1234 5678" };

        var response = await client.PatchAsJsonAsync("/api/v1/users/me", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyDashboard_ReturnsUpcomingAndHistorySummary()
    {
        var client = _factory.CreateClient();
        var suffix = $"dashboard-user-{Guid.NewGuid()}";
        var login = await LoginWithUserAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var now = DateTime.UtcNow;
        Guid upcomingId;
        Guid historyId;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

            var upcoming = Reservation.Create(complex.Id, court.Id, login.User.Id, now.AddHours(2), now.AddHours(3), ReservationSource.Web);
            upcoming.Confirm();
            upcomingId = upcoming.Id;

            var history = Reservation.Create(complex.Id, court.Id, login.User.Id, now.AddHours(-2), now.AddHours(-1), ReservationSource.Web);
            history.Confirm();
            historyId = history.Id;

            await context.Reservations.AddRangeAsync(upcoming, history);
            await context.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/v1/users/me/dashboard?upcomingPageSize=5&historyPageSize=3");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserDashboardInfo>();
        Assert.NotNull(result);
        Assert.Equal(login.User.FullName, result.User.FullName);
        Assert.Equal(1, result.UpcomingReservations.TotalItems);
        Assert.Equal(upcomingId, result.UpcomingReservations.Items[0].Id);
        Assert.Equal(1, result.HistorySummary.TotalItems);
        Assert.Single(result.HistorySummary.RecentReservations);
        Assert.Equal(historyId, result.HistorySummary.RecentReservations[0].Id);
    }

    [Fact]
    public async Task ComplexUsers_List_ReturnsUsersWithReservations()
    {
        var (adminSuffix, complexId, customerId, _) = await SeedScenarioAsync();

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/complexes/{complexId}/users");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ComplexUserInfo>>();

        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(customerId, result.Items[0].Id);
        Assert.False(result.Items[0].IsBlocked);
    }

    [Fact]
    public async Task ComplexUsers_Search_ReturnsAllActiveUsers()
    {
        var (adminSuffix, complexId, customerId, _) = await SeedScenarioAsync();

        var suffix = Guid.NewGuid();
        var fullName = $"Searchable User {suffix}";
        var email = $"searchable-{suffix}@test.com";

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
            var otherUser = User.CreateFromGoogle(
                Guid.NewGuid(),
                $"sub-search-{suffix}",
                email,
                fullName);
            otherUser.CompleteProfile("+54 11 1234 5678");
            await context.Users.AddAsync(otherUser);
            await context.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/complexes/{complexId}/users/search?search={Uri.EscapeDataString(fullName)}&pageSize=10");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ComplexUserInfo>>();

        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(fullName, result.Items[0].FullName);
        Assert.Equal("+54 11 1234 5678", result.Items[0].PhoneNumber);
        Assert.False(result.Items[0].IsBlocked);
    }

    [Fact]
    public async Task ComplexUsers_Search_WithNormalizedPhoneDigits_ReturnsMatchingUser()
    {
        var (adminSuffix, complexId, _, _) = await SeedScenarioAsync();

        var suffix = new Random().Next(10000000, 99999999);
        var fullName = $"Phone User {suffix}";
        var phone = $"+54 {suffix}";
        var searchDigits = new string(phone.Where(char.IsDigit).ToArray());

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
            var otherUser = User.CreateFromGoogle(
                Guid.NewGuid(),
                $"sub-phone-{suffix}",
                $"phone-{suffix}@test.com",
                fullName);
            otherUser.CompleteProfile(phone);
            await context.Users.AddAsync(otherUser);
            await context.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/complexes/{complexId}/users/search?search={Uri.EscapeDataString(searchDigits)}&pageSize=10");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ComplexUserInfo>>();

        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(fullName, result.Items[0].FullName);
        Assert.Equal(phone, result.Items[0].PhoneNumber);
    }

    [Fact]
    public async Task ComplexUsers_GetReservations_ReturnsUserHistory()
    {
        var (adminSuffix, complexId, customerId, _) = await SeedScenarioAsync();

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/complexes/{complexId}/users/{customerId}/reservations");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReservationInfo>>();

        Assert.NotNull(result);
        Assert.Single(result!.Items);
    }

    [Fact]
    public async Task BlockedUsers_BlockAndUnblock_Flow_Succeeds()
    {
        var (adminSuffix, complexId, customerId, _) = await SeedScenarioAsync();

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, adminSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var blockResponse = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{complexId}/blocked-users",
            new BlockUserRequest { UserId = customerId, Reason = "No-show" });

        Assert.Equal(HttpStatusCode.Created, blockResponse.StatusCode);
        var block = await blockResponse.Content.ReadFromJsonAsync<BlockedUserInfo>();
        Assert.NotNull(block);

        var listResponse = await client.GetAsync($"/api/v1/complexes/{complexId}/users");
        listResponse.EnsureSuccessStatusCode();
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<ComplexUserInfo>>();
        Assert.Contains(list!.Items, u => u.Id == customerId && u.IsBlocked);

        var unblockResponse = await client.DeleteAsync($"/api/v1/complexes/{complexId}/blocked-users/{block!.Id}");
        unblockResponse.EnsureSuccessStatusCode();

        listResponse = await client.GetAsync($"/api/v1/complexes/{complexId}/users");
        list = await listResponse.Content.ReadFromJsonAsync<PagedResult<ComplexUserInfo>>();
        Assert.Contains(list!.Items, u => u.Id == customerId && !u.IsBlocked);
    }

    private async Task<(string AdminSuffix, Guid ComplexId, Guid CustomerId, Guid CourtId)> SeedScenarioAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var adminSuffix = $"users-admin-{Guid.NewGuid()}";
        var adminUser = User.CreateFromGoogle(
            Guid.NewGuid(),
            $"sub-{adminSuffix}",
            $"user-{adminSuffix}@test.com",
            $"Admin {adminSuffix}");
        adminUser.AddRole(Role.User);
        adminUser.AddRole(Role.ComplexAdmin);

        var customerUser = User.CreateFromGoogle(
            Guid.NewGuid(),
            $"sub-customer-{Guid.NewGuid()}",
            $"user-customer-{Guid.NewGuid()}@test.com",
            "Customer User");
        customerUser.AddRole(Role.User);

        var complex = SportsComplex.Create(
            $"Complex {Guid.NewGuid()}",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            $"complex-{Guid.NewGuid()}@test.com");

        var sport = Sport.Create($"Sport {Guid.NewGuid()}");
        var court = Court.Create(complex.Id, $"Court {Guid.NewGuid()}", "Court", "Synthetic", true, [sport.Id]);

        var startAt = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);
        var endAt = startAt.AddHours(1);
        var reservation = Reservation.Create(complex.Id, court.Id, customerUser.Id, startAt, endAt, ReservationSource.Web);
        reservation.Confirm();

        var administrator = ComplexAdministrator.Create(complex.Id, adminUser.Id, Role.ComplexAdmin);

        await context.Sports.AddAsync(sport);
        await context.Users.AddRangeAsync(adminUser, customerUser);
        await context.SportsComplexes.AddAsync(complex);
        await context.Courts.AddAsync(court);
        await context.Reservations.AddAsync(reservation);
        await context.ComplexAdministrators.AddAsync(administrator);
        await context.SaveChangesAsync();

        return (adminSuffix, complex.Id, customerUser.Id, court.Id);
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
            Name = $"Dashboard Complex {Guid.NewGuid()}",
            Description = "Complex",
            Address = "Address",
            City = "Montevideo",
            PhoneNumber = "+598 99 123 456",
            Email = $"dashboard-{Guid.NewGuid()}@test.com",
            TimeZoneId = "America/Montevideo"
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

    private static async Task<string> LoginAsync(HttpClient client)
    {
        return await LoginAsync(client, Guid.NewGuid().ToString());
    }

    private static async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        var idToken = $"valid-token-{suffix}";
        var loginRequest = new GoogleLoginRequest { IdToken = idToken };
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", loginRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return result!.AccessToken;
    }
}
