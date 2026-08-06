using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Contracts.Courts;

namespace Mova.IntegrationTests.Courts;

[Collection("AuthIntegrationTests")]
public sealed class BusinessHoursControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public BusinessHoursControllerTests(MovaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedHours()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"business-hours-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);

        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{complex.Id}/business-hours", new UpdateBusinessHoursRequest
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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<BusinessHoursInfo>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(DayOfWeek.Monday, result[0].DayOfWeek);
    }

    [Fact]
    public async Task Get_AfterUpdate_ReturnsHours()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, $"business-hours-get-admin-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var complex = await CreateComplexAsync(client);
        await client.PutAsJsonAsync($"/api/v1/complexes/{complex.Id}/business-hours", new UpdateBusinessHoursRequest
        {
            Hours =
            [
                new BusinessHoursRequest
                {
                    DayOfWeek = DayOfWeek.Tuesday,
                    OpeningTime = TimeSpan.FromHours(9),
                    ClosingTime = TimeSpan.FromHours(21),
                    IsClosed = false
                }
            ]
        });

        var response = await client.GetAsync($"/api/v1/complexes/{complex.Id}/business-hours");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<BusinessHoursInfo>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(DayOfWeek.Tuesday, result[0].DayOfWeek);
    }

    [Fact]
    public async Task Update_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/v1/complexes/{Guid.NewGuid()}/business-hours", new UpdateBusinessHoursRequest
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

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
            Name = $"Business Hours Complex {Guid.NewGuid()}",
            Description = "Complex",
            Address = "Address",
            City = "Montevideo",
            PhoneNumber = "+598 99 123 456",
            Email = $"business-hours-{Guid.NewGuid()}@test.com"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SportsComplexInfo>())!;
    }
}
