using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mova.Contracts.Auth;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;
using Mova.Contracts.Users;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;
using Xunit;

namespace Mova.IntegrationTests.Users;

[Collection("AuthIntegrationTests")]
public class UserFavoritesControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public UserFavoritesControllerTests(MovaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddListAndRemoveFavorite_Flow_Succeeds()
    {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client, $"fav-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);

        var addResponse = await client.PutAsync($"/api/v1/users/me/favorites/{complex.Id}", null);
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        var idsResponse = await client.GetAsync("/api/v1/users/me/favorites/ids");
        idsResponse.EnsureSuccessStatusCode();
        var ids = await idsResponse.Content.ReadFromJsonAsync<FavoriteComplexIdsInfo>();
        Assert.NotNull(ids);
        Assert.Contains(complex.Id, ids.Items);

        var listResponse = await client.GetAsync("/api/v1/users/me/favorites");
        listResponse.EnsureSuccessStatusCode();
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<FavoriteComplexInfo>>();
        Assert.NotNull(list);
        Assert.Single(list.Items);
        Assert.Equal(complex.Id, list.Items[0].Id);
        Assert.Equal(complex.Name, list.Items[0].Name);

        var deleteResponse = await client.DeleteAsync($"/api/v1/users/me/favorites/{complex.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var idsAfter = await (await client.GetAsync("/api/v1/users/me/favorites/ids"))
            .Content.ReadFromJsonAsync<FavoriteComplexIdsInfo>();
        Assert.NotNull(idsAfter);
        Assert.DoesNotContain(complex.Id, idsAfter.Items);
    }

    [Fact]
    public async Task AddFavorite_IsIdempotent_AndDoesNotDuplicate()
    {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client, $"fav-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);

        await client.PutAsync($"/api/v1/users/me/favorites/{complex.Id}", null);
        var second = await client.PutAsync($"/api/v1/users/me/favorites/{complex.Id}", null);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);

        var list = await (await client.GetAsync("/api/v1/users/me/favorites"))
            .Content.ReadFromJsonAsync<PagedResult<FavoriteComplexInfo>>();
        Assert.NotNull(list);
        Assert.Equal(1, list.TotalItems);
    }

    [Fact]
    public async Task RemoveFavorite_IsIdempotent()
    {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client, $"fav-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var complex = await CreateComplexAsync(client);
        await client.PutAsync($"/api/v1/users/me/favorites/{complex.Id}", null);

        var first = await client.DeleteAsync($"/api/v1/users/me/favorites/{complex.Id}");
        var second = await client.DeleteAsync($"/api/v1/users/me/favorites/{complex.Id}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_WithNonexistentComplex_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client, $"fav-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await client.PutAsync($"/api/v1/users/me/favorites/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_WithInactiveComplex_ReturnsConflict_AndFavoriteRemainsListedWhenDeactivatedLater()
    {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client, $"fav-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var active = await CreateComplexAsync(client);
        await client.PutAsync($"/api/v1/users/me/favorites/{active.Id}", null);

        var inactive = await CreateComplexAsync(client);
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();
            var entity = await context.SportsComplexes.FirstAsync(x => x.Id == inactive.Id);
            entity.Deactivate();
            await context.SaveChangesAsync();
        }

        var addInactive = await client.PutAsync($"/api/v1/users/me/favorites/{inactive.Id}", null);
        Assert.Equal(HttpStatusCode.Conflict, addInactive.StatusCode);

        var list = await (await client.GetAsync("/api/v1/users/me/favorites"))
            .Content.ReadFromJsonAsync<PagedResult<FavoriteComplexInfo>>();
        Assert.NotNull(list);
        Assert.Single(list.Items);
        Assert.Equal(active.Id, list.Items[0].Id);
    }

    [Fact]
    public async Task Favorites_AreIsolatedPerUser_AndIndependent()
    {
        var clientA = _factory.CreateClient();
        var loginA = await LoginAsync(clientA, $"fav-a-{Guid.NewGuid()}");
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginA.AccessToken);
        var complex = await CreateComplexAsync(clientA);

        var clientB = _factory.CreateClient();
        var loginB = await LoginAsync(clientB, $"fav-b-{Guid.NewGuid()}");
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginB.AccessToken);

        await clientA.PutAsync($"/api/v1/users/me/favorites/{complex.Id}", null);

        var idsB = await (await clientB.GetAsync("/api/v1/users/me/favorites/ids"))
            .Content.ReadFromJsonAsync<FavoriteComplexIdsInfo>();
        Assert.NotNull(idsB);
        Assert.DoesNotContain(complex.Id, idsB.Items);

        var addB = await clientB.PutAsync($"/api/v1/users/me/favorites/{complex.Id}", null);
        Assert.Equal(HttpStatusCode.NoContent, addB.StatusCode);

        await clientA.DeleteAsync($"/api/v1/users/me/favorites/{complex.Id}");

        var idsBAfter = await (await clientB.GetAsync("/api/v1/users/me/favorites/ids"))
            .Content.ReadFromJsonAsync<FavoriteComplexIdsInfo>();
        Assert.NotNull(idsBAfter);
        Assert.Contains(complex.Id, idsBAfter.Items);
    }

    [Fact]
    public async Task Favorites_AreOrderedByMostRecent()
    {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client, $"fav-{Guid.NewGuid()}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var first = await CreateComplexAsync(client);
        var second = await CreateComplexAsync(client);

        await client.PutAsync($"/api/v1/users/me/favorites/{first.Id}", null);
        await Task.Delay(20);
        await client.PutAsync($"/api/v1/users/me/favorites/{second.Id}", null);

        var list = await (await client.GetAsync("/api/v1/users/me/favorites"))
            .Content.ReadFromJsonAsync<PagedResult<FavoriteComplexInfo>>();
        Assert.NotNull(list);
        Assert.Equal(2, list.TotalItems);
        Assert.Equal(second.Id, list.Items[0].Id);
        Assert.Equal(first.Id, list.Items[1].Id);
    }

    [Fact]
    public async Task Favorites_WithoutAuthorization_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/users/me/favorites")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/users/me/favorites/ids")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsync($"/api/v1/users/me/favorites/{Guid.NewGuid()}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync($"/api/v1/users/me/favorites/{Guid.NewGuid()}")).StatusCode);
    }

    private static async Task<GoogleLoginResponse> LoginAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GoogleLoginResponse>())!;
    }

    private static async Task<SportsComplexInfo> CreateComplexAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/complexes", new CreateComplexRequest
        {
            Name = $"Favorite Complex {Guid.NewGuid()}",
            Description = "Complex",
            Address = "Address",
            City = "Montevideo",
            PhoneNumber = "+598 99 123 456",
            Email = $"favorite-{Guid.NewGuid()}@test.com",
            TimeZoneId = "America/Montevideo"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SportsComplexInfo>())!;
    }
}
