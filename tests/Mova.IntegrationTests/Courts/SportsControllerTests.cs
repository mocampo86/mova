using System.Net;
using System.Net.Http.Json;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Mova.IntegrationTests.Courts;

[Collection("AuthIntegrationTests")]
public sealed class SportsControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public SportsControllerTests(MovaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetActive_ReturnsActiveSports()
    {
        var sportName = $"Sport-{Guid.NewGuid()}";
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
            context.Sports.Add(Sport.Create(sportName));
            await context.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/sports");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<SportInfo>>();
        Assert.NotNull(result);
        Assert.Contains(result, s => s.Name == sportName);
    }
}
