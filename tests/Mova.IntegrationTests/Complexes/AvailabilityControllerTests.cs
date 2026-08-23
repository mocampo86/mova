using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;
using Mova.IntegrationTests.Authentication;

namespace Mova.IntegrationTests.Complexes;

[Collection("AuthIntegrationTests")]
public sealed class AvailabilityControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public AvailabilityControllerTests(MovaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAvailability_WithRuleAndBusinessHours_ReturnsSlots()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await SetBusinessHoursAsync(client, complex.Id);
        await SetAvailabilityRuleAsync(client, complex.Id, court.Id);

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/availability?courtId={court.Id}&date=2026-08-10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CourtAvailabilitySlotInfo>>();
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task GetAvailability_WithReservation_OverlappingSlot_RemovesSlot()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-reserved-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await SetBusinessHoursAsync(client, complex.Id);
        await SetAvailabilityRuleAsync(client, complex.Id, court.Id);
        await SeedReservationAsync(complex.Id, court.Id, new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc));

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/availability?courtId={court.Id}&date=2026-08-10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CourtAvailabilitySlotInfo>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, s => s.StartAt == new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetAvailability_WithCourtBlock_OverlappingSlot_RemovesSlot()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-blocked-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await SetBusinessHoursAsync(client, complex.Id);
        await SetAvailabilityRuleAsync(client, complex.Id, court.Id);
        await SeedCourtBlockAsync(complex.Id, court.Id, new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 10, 11, 0, 0, DateTimeKind.Utc));

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/availability?courtId={court.Id}&date=2026-08-10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CourtAvailabilitySlotInfo>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, s => s.StartAt == new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetAvailability_WithMonToSatSchedule_ReturnsSlotsForSaturday()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-week-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await SetBusinessHoursForWeekAsync(client, complex.Id);
        await SetAvailabilityRulesForWeekAsync(client, complex.Id, court.Id);

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/availability?courtId={court.Id}&date=2026-08-15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CourtAvailabilitySlotInfo>>();
        Assert.NotNull(result);
        Assert.Equal(14, result.Count);
    }

    [Fact]
    public async Task GetAvailability_ForInactiveComplex_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-inactive-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await client.PatchAsJsonAsync($"/api/v1/complexes/{complex.Id}/status", new UpdateComplexStatusRequest { Status = "Inactive" });

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/availability?courtId={court.Id}&date=2026-08-10");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_WithoutCourtId_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var complex = await CreateComplexAsync(client, authorized: false);

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/availability?date=2026-08-10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_WithOvernightRuleAndBusinessHours_ReturnsSlotsAcrossMidnight()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"availability-overnight-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await SetOvernightBusinessHoursAsync(client, complex.Id);
        await SetOvernightAvailabilityRuleAsync(client, complex.Id, court.Id);

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/availability?courtId={court.Id}&date=2026-08-10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CourtAvailabilitySlotInfo>>();
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal(new DateTime(2026, 8, 10, 22, 0, 0, DateTimeKind.Utc), result[0].StartAt);
        Assert.Equal(new DateTime(2026, 8, 11, 2, 0, 0, DateTimeKind.Utc), result[3].EndAt);
    }

    private static async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GoogleLoginResponse>())!.AccessToken;
    }

    private static async Task<SportsComplexInfo> CreateComplexAsync(HttpClient client, bool authorized = true)
    {
        if (!authorized)
        {
            var suffix = $"public-complex-{Guid.NewGuid()}";
            var token = await LoginAsync(client, suffix);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

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

    private static async Task SetBusinessHoursAsync(HttpClient client, Guid complexId)
    {
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complexId}/business-hours", new UpdateBusinessHoursRequest
        {
            Hours =
            [
                new BusinessHoursRequest
                {
                    DayOfWeek = DayOfWeek.Monday,
                    OpeningTime = TimeSpan.FromHours(8),
                    ClosingTime = TimeSpan.FromHours(22),
                    IsClosed = false
                }
            ]
        });
        response.EnsureSuccessStatusCode();
    }

    private static async Task SetBusinessHoursForWeekAsync(HttpClient client, Guid complexId)
    {
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complexId}/business-hours", new UpdateBusinessHoursRequest
        {
            Hours = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                DayOfWeek.Friday, DayOfWeek.Saturday
            }
            .Select(d => new BusinessHoursRequest
            {
                DayOfWeek = d,
                OpeningTime = TimeSpan.FromHours(8),
                ClosingTime = TimeSpan.FromHours(22),
                IsClosed = false
            })
            .ToList()
        });
        response.EnsureSuccessStatusCode();
    }

    private static async Task SetAvailabilityRuleAsync(HttpClient client, Guid complexId, Guid courtId)
    {
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complexId}/courts/{courtId}/availability", new UpdateCourtAvailabilityRulesRequest
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
        response.EnsureSuccessStatusCode();
    }

    private static async Task SetAvailabilityRulesForWeekAsync(HttpClient client, Guid complexId, Guid courtId)
    {
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complexId}/courts/{courtId}/availability", new UpdateCourtAvailabilityRulesRequest
        {
            Rules = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                DayOfWeek.Friday, DayOfWeek.Saturday
            }
            .Select(d => new CreateCourtAvailabilityRuleRequest
            {
                DayOfWeek = d,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(22),
                SlotDurationMinutes = 60,
                IsActive = true
            })
            .ToList()
        });
        response.EnsureSuccessStatusCode();
    }

    private static async Task SetOvernightBusinessHoursAsync(HttpClient client, Guid complexId)
    {
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complexId}/business-hours", new UpdateBusinessHoursRequest
        {
            Hours =
            [
                new BusinessHoursRequest
                {
                    DayOfWeek = DayOfWeek.Monday,
                    OpeningTime = TimeSpan.FromHours(22),
                    ClosingTime = TimeSpan.FromHours(2),
                    IsClosed = false
                }
            ]
        });
        response.EnsureSuccessStatusCode();
    }

    private static async Task SetOvernightAvailabilityRuleAsync(HttpClient client, Guid complexId, Guid courtId)
    {
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complexId}/courts/{courtId}/availability", new UpdateCourtAvailabilityRulesRequest
        {
            Rules =
            [
                new CreateCourtAvailabilityRuleRequest
                {
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = TimeSpan.FromHours(22),
                    EndTime = TimeSpan.FromHours(2),
                    SlotDurationMinutes = 60,
                    IsActive = true
                }
            ]
        });
        response.EnsureSuccessStatusCode();
    }

    private async Task SeedReservationAsync(Guid complexId, Guid courtId, DateTime startAt, DateTime endAt)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}@test.com", "Test User");
        var reservation = Reservation.Create(complexId, courtId, user.Id, startAt, endAt, ReservationSource.Web);

        context.Users.Add(user);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();
    }

    private async Task SeedCourtBlockAsync(Guid complexId, Guid courtId, DateTime startAt, DateTime endAt)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{Guid.NewGuid()}", $"block-user-{Guid.NewGuid()}@test.com", "Test User");
        var block = CourtBlock.Create(complexId, courtId, startAt, endAt, user.Id);

        context.Users.Add(user);
        context.CourtBlocks.Add(block);
        await context.SaveChangesAsync();
    }
}
