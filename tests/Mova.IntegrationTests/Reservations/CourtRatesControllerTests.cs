using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;
using Mova.Contracts.CourtRates;
using Mova.Contracts.Reservations;
using Xunit;

namespace Mova.IntegrationTests.Reservations;

[Collection("AuthIntegrationTests")]
public sealed class CourtRatesControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public CourtRatesControllerTests(MovaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateAndListRates_AsAdmin_ReturnsConfiguredRates()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"rates-list-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var createResponse = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/courts/{court.Id}/rates",
            new CreateCourtRateRequest
            {
                DaysOfWeek = [1, 2, 3, 4, 5],
                StartTime = new TimeSpan(17, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                AmountPerHour = 1200m
            });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/v1/complexes/{complex.Id}/courts/{court.Id}/rates");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var rates = await listResponse.Content.ReadFromJsonAsync<List<CourtRateInfo>>();
        var rate = Assert.Single(rates!);
        Assert.Equal(1200m, rate.AmountPerHour);
        Assert.Equal([1, 2, 3, 4, 5], rate.DaysOfWeek);
        Assert.True(rate.IsActive);
    }

    [Fact]
    public async Task CreateRate_WithOverlap_ReturnsRateOverlapError()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"rates-overlap-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await CreateRateAsync(client, complex.Id, court.Id, [0, 1, 2, 3, 4, 5, 6], 0, 24, 100m);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/courts/{court.Id}/rates",
            new CreateCourtRateRequest
            {
                DaysOfWeek = [1],
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                AmountPerHour = 500m
            });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("RATE_OVERLAP", body);
    }

    [Fact]
    public async Task RateEndpoints_AsAdminOfAnotherComplex_ReturnForbidden()
    {
        var client = _factory.CreateClient();
        var ownerToken = await LoginAsync(client, $"rates-owner-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);

        var outsiderToken = await LoginAsync(client, $"rates-outsider-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);
        var otherComplex = await CreateComplexAsync(client);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/courts/{court.Id}/rates",
            new CreateCourtRateRequest
            {
                DaysOfWeek = [1],
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                AmountPerHour = 500m
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPriceQuote_Anonymous_ReturnsBreakdownPerSegment()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"quote-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await CreateRateAsync(client, complex.Id, court.Id, [0, 1, 2, 3, 4, 5, 6], 17, 18, 1200m);
        await CreateRateAsync(client, complex.Id, court.Id, [0, 1, 2, 3, 4, 5, 6], 18, 19, 1800m);

        // Monday 2026-08-10 17:00-19:00 local (UTC-3) => 20:00-22:00 UTC.
        var startAt = new DateTime(2026, 8, 10, 20, 0, 0, DateTimeKind.Utc);
        var endAt = new DateTime(2026, 8, 10, 22, 0, 0, DateTimeKind.Utc);

        var response = await client.GetAsync(
            $"/api/v1/complexes/{complex.Id}/courts/{court.Id}/price-quote?startAt={startAt:O}&endAt={endAt:O}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var quote = await response.Content.ReadFromJsonAsync<PriceQuoteInfo>();
        Assert.NotNull(quote);
        Assert.True(quote.IsFullyCovered);
        Assert.Equal(3000m, quote.TotalAmount);
        Assert.Equal("UYU", quote.Currency);
        Assert.Equal(2, quote.Lines.Count);
    }

    [Fact]
    public async Task GetPriceQuote_WithoutCoverage_ReportsGaps()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"quote-gap-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await CreateRateAsync(client, complex.Id, court.Id, [0, 1, 2, 3, 4, 5, 6], 17, 18, 1200m);

        var startAt = new DateTime(2026, 8, 10, 20, 0, 0, DateTimeKind.Utc); // 17:00 local
        var endAt = new DateTime(2026, 8, 10, 22, 0, 0, DateTimeKind.Utc);   // 19:00 local

        var response = await client.GetAsync(
            $"/api/v1/complexes/{complex.Id}/courts/{court.Id}/price-quote?startAt={startAt:O}&endAt={endAt:O}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var quote = await response.Content.ReadFromJsonAsync<PriceQuoteInfo>();
        Assert.NotNull(quote);
        Assert.False(quote.IsFullyCovered);
        Assert.Single(quote.Gaps);
        Assert.Equal(1200m, quote.TotalAmount);
    }

    [Fact]
    public async Task CreateReservation_WithoutPriceCoverage_ReturnsPriceNotConfigured()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"res-no-rate-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id); // no rate configured

        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = new DateTime(2026, 9, 7, 20, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 9, 7, 21, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("PRICE_NOT_CONFIGURED", body);
    }

    [Fact]
    public async Task CreateReservation_WithStaleExpectedPrice_ReturnsPriceChanged()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"res-stale-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        await CreateRateAsync(client, complex.Id, court.Id, [0, 1, 2, 3, 4, 5, 6], 0, 24, 100m);

        var response = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = new DateTime(2026, 9, 7, 20, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 9, 7, 21, 0, 0, DateTimeKind.Utc),
            ExpectedTotalPrice = 50m,
            ExpectedCurrency = "UYU"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("PRICE_CHANGED", body);
        Assert.Contains("100", body);
    }

    [Fact]
    public async Task UpdateRate_DoesNotAlterExistingReservationPrice()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"rate-history-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var complex = await CreateComplexAsync(client);
        var court = await CreateCourtAsync(client, complex.Id);
        var rate = await CreateRateAsync(client, complex.Id, court.Id, [0, 1, 2, 3, 4, 5, 6], 0, 24, 100m);

        var createResponse = await client.PostAsJsonAsync($"/api/v1/complexes/{complex.Id}/reservations/me", new CreateMyReservationRequest
        {
            CourtId = court.Id,
            StartAt = new DateTime(2026, 9, 7, 20, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 9, 7, 21, 0, 0, DateTimeKind.Utc),
            ExpectedTotalPrice = 100m,
            ExpectedCurrency = "UYU"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var reservation = await createResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(reservation);
        Assert.Equal(100m, reservation.TotalPrice);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/complexes/{complex.Id}/courts/{court.Id}/rates/{rate.Id}",
            new UpdateCourtRateRequest
            {
                DaysOfWeek = [0, 1, 2, 3, 4, 5, 6],
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromHours(24),
                AmountPerHour = 9999m
            });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/complexes/{complex.Id}/reservations/{reservation.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var reloaded = await getResponse.Content.ReadFromJsonAsync<ReservationInfo>();
        Assert.NotNull(reloaded);
        Assert.Equal(100m, reloaded.TotalPrice);
        Assert.Equal("UYU", reloaded.Currency);
        var line = Assert.Single(reloaded.PriceLines);
        Assert.Equal(100m, line.HourlyRate);
    }

    private static async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return login!.AccessToken;
    }

    private static async Task<SportsComplexInfo> CreateComplexAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/complexes", new CreateComplexRequest
        {
            Name = $"Rates Complex {Guid.NewGuid()}",
            Description = "Complex",
            Address = "Address",
            City = "Montevideo",
            PhoneNumber = "+598 99 123 456",
            Email = $"rates-{Guid.NewGuid()}@test.com",
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

    private static async Task<CourtRateInfo> CreateRateAsync(
        HttpClient client,
        Guid complexId,
        Guid courtId,
        int[] days,
        int startHour,
        int endHour,
        decimal amountPerHour)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/v1/complexes/{complexId}/courts/{courtId}/rates",
            new CreateCourtRateRequest
            {
                DaysOfWeek = days,
                StartTime = TimeSpan.FromHours(startHour),
                EndTime = TimeSpan.FromHours(endHour),
                AmountPerHour = amountPerHour
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CourtRateInfo>())!;
    }
}
