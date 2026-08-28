using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Mova.Contracts.Audit;
using Mova.Contracts.Auth;
using Mova.Contracts.Common;
using Mova.Contracts.Complexes;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;
using Xunit;

namespace Mova.IntegrationTests.Authentication;

[Collection("AuthIntegrationTests")]
public class AdminControllerTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public AdminControllerTests(MovaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SuperAdminEndpoint_WithSuperAdminRole_ReturnsOk()
    {
        var suffix = $"super-{Guid.NewGuid()}";
        await SeedUserAsync(suffix, [Role.SuperAdmin]);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/admin/super");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SuperAdminEndpoint_WithUserRole_ReturnsForbidden()
    {
        var suffix = $"user-{Guid.NewGuid()}";
        await SeedUserAsync(suffix, [Role.User]);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/admin/super");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdminEndpoint_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/super");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ComplexAdminEndpoint_WithAssignedComplex_ReturnsOk()
    {
        var suffix = $"complex-admin-{Guid.NewGuid()}";
        var seededComplexIds = await SeedUserAsync(suffix, [Role.User, Role.ComplexAdmin], [Guid.NewGuid()]);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/admin/complexes/{seededComplexIds[0]}");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ComplexAdminEndpoint_WithDifferentComplex_ReturnsForbidden()
    {
        var suffix = $"complex-admin-other-{Guid.NewGuid()}";
        await SeedUserAsync(suffix, [Role.User, Role.ComplexAdmin], [Guid.NewGuid()]);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/admin/complexes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ComplexAdminEndpoint_WithSuperAdmin_ReturnsOk()
    {
        var suffix = $"super-complex-{Guid.NewGuid()}";
        await SeedUserAsync(suffix, [Role.SuperAdmin]);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/admin/complexes/{Guid.NewGuid()}");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ComplexAdminEndpoint_WithInactiveAdmin_ReturnsForbidden()
    {
        var suffix = $"complex-admin-inactive-{Guid.NewGuid()}";
        var seededComplexIds = await SeedUserAsync(suffix, [Role.User, Role.ComplexAdmin], [Guid.NewGuid()], isActive: false);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/admin/complexes/{seededComplexIds[0]}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllComplexes_WithSuperAdminRole_ReturnsAllComplexesIncludingInactive()
    {
        var suffix = $"super-list-{Guid.NewGuid()}";
        var seededComplexIds = await SeedUserAsync(suffix, [Role.SuperAdmin], [Guid.NewGuid()]);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await DeactivateComplexAsync(seededComplexIds[0]);

        var response = await client.GetAsync("/api/v1/admin/complexes");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<SportsComplexInfo>>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, c => c.Id == seededComplexIds[0]);
        Assert.Contains(result.Items, c => c.Status == "Inactive");
    }

    [Fact]
    public async Task GetAllComplexes_WithUserRole_ReturnsForbidden()
    {
        var suffix = $"user-list-{Guid.NewGuid()}";
        await SeedUserAsync(suffix, [Role.User]);
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/admin/complexes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllComplexes_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/complexes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsSuperAdmin_TogglesComplexStatus()
    {
        var ownerSuffix = $"owner-{Guid.NewGuid()}";
        var superSuffix = $"super-{Guid.NewGuid()}";
        var seededComplexIds = await SeedUserAsync(ownerSuffix, [Role.User, Role.ComplexAdmin], [Guid.NewGuid()]);
        await SeedUserAsync(superSuffix, [Role.SuperAdmin]);

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, superSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var deactivateResponse = await client.PatchAsJsonAsync(
            $"/api/v1/complexes/{seededComplexIds[0]}/status",
            new UpdateComplexStatusRequest { Status = "Inactive" });

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(deactivated);
        Assert.Equal("Inactive", deactivated.Status);
    }

    [Fact]
    public async Task GetAuditLogs_AsSuperAdmin_ReturnsAuditLogs()
    {
        var superSuffix = $"super-audit-{Guid.NewGuid()}";
        await SeedUserAsync(superSuffix, [Role.SuperAdmin]);
        var entityId = Guid.NewGuid().ToString();
        await SeedAuditLogAsync("SportsComplex.Create", "SportsComplex", entityId);

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, superSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/admin/audit-logs");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AuditLogInfo>>();
        Assert.NotNull(result);
        Assert.True(result.TotalItems >= 1);
        Assert.Contains(result.Items, x => x.EntityId == entityId);
    }

    [Fact]
    public async Task GetAuditLogs_WithFilters_ReturnsMatchingAuditLogs()
    {
        var superSuffix = $"super-audit-filter-{Guid.NewGuid()}";
        await SeedUserAsync(superSuffix, [Role.SuperAdmin]);
        var entityId = Guid.NewGuid().ToString();
        await SeedAuditLogAsync("Court.Create", "Court", entityId);
        await SeedAuditLogAsync("SportsComplex.Create", "SportsComplex", Guid.NewGuid().ToString());

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, superSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/admin/audit-logs?action=Court.Create&entityType=Court");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AuditLogInfo>>();
        Assert.NotNull(result);
        Assert.True(result.TotalItems >= 1);
        Assert.All(result.Items, x =>
        {
            Assert.Equal("Court.Create", x.Action);
            Assert.Equal("Court", x.EntityType);
        });
    }

    [Fact]
    public async Task GetAuditLogs_AsUser_ReturnsForbidden()
    {
        var suffix = $"user-audit-{Guid.NewGuid()}";
        await SeedUserAsync(suffix, [Role.User]);

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/admin/audit-logs");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_WithDateRangeFilter_ReturnsAuditLogs()
    {
        var superSuffix = $"super-audit-date-{Guid.NewGuid()}";
        await SeedUserAsync(superSuffix, [Role.SuperAdmin]);
        var entityId = Guid.NewGuid().ToString();
        await SeedAuditLogAsync("SportsComplex.Create", "SportsComplex", entityId);

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, superSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var from = DateTime.UtcNow.AddDays(-2).ToString("o");
        var to = DateTime.UtcNow.AddHours(1).ToString("o");

        var response = await client.GetAsync($"/api/v1/admin/audit-logs?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AuditLogInfo>>();
        Assert.NotNull(result);
        Assert.True(result.TotalItems >= 1);
        Assert.Contains(result.Items, x => x.EntityId == entityId);
    }

    [Fact]
    public async Task GetAuditLogs_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/audit-logs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateComplexStatus_AsSuperAdmin_CreatesAuditLog()
    {
        var ownerSuffix = $"owner-audit-{Guid.NewGuid()}";
        var superSuffix = $"super-audit-{Guid.NewGuid()}";
        var seededComplexIds = await SeedUserAsync(ownerSuffix, [Role.User, Role.ComplexAdmin], [Guid.NewGuid()]);
        await SeedUserAsync(superSuffix, [Role.SuperAdmin]);

        var client = _factory.CreateClient();
        var token = await LoginAsync(client, superSuffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/complexes/{seededComplexIds[0]}/status",
            new UpdateComplexStatusRequest { Status = "Inactive" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auditResponse = await client.GetAsync($"/api/v1/admin/audit-logs?entityType=SportsComplex&entityId={seededComplexIds[0]}");
        auditResponse.EnsureSuccessStatusCode();

        var result = await auditResponse.Content.ReadFromJsonAsync<PagedResult<AuditLogInfo>>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, x =>
            x.Action == "SportsComplex.UpdateStatus" &&
            x.EntityType == "SportsComplex" &&
            x.EntityId == seededComplexIds[0].ToString());
    }

    private async Task DeactivateComplexAsync(Guid complexId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var complex = await context.SportsComplexes.FindAsync(complexId);
        if (complex is not null)
        {
            complex.Deactivate();
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedAuditLogAsync(string action, string entityType, string entityId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var auditLog = AuditLog.Create(
            null,
            null,
            action,
            entityType,
            entityId,
            new { });

        await context.AuditLogs.AddAsync(auditLog);
        await context.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<Guid>> SeedUserAsync(
        string suffix,
        IReadOnlyCollection<Role> roles,
        IReadOnlyCollection<Guid>? complexIds = null,
        bool isActive = true)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var user = User.CreateFromGoogle(Guid.NewGuid(), $"sub-{suffix}", $"user-{suffix}@test.com", $"Test {suffix}");
        foreach (var role in roles)
        {
            user.AddRole(role);
        }

        await context.Users.AddAsync(user);

        var createdComplexIds = new List<Guid>();

        if (complexIds is not null)
        {
            foreach (var complexId in complexIds)
            {
                var sportsComplex = SportsComplex.Create(
                    $"Complex {complexId}",
                    "Test complex",
                    "Test address",
                    "Test city",
                    null,
                    null,
                    "+54 11 1234 5678",
                    $"test-{complexId}@example.com");

                createdComplexIds.Add(sportsComplex.Id);

                var administrator = ComplexAdministrator.Create(sportsComplex.Id, user.Id, Role.ComplexAdmin);
                if (!isActive)
                {
                    administrator.Deactivate();
                }

                await context.SportsComplexes.AddAsync(sportsComplex);
                await context.ComplexAdministrators.AddAsync(administrator);
            }
        }

        await context.SaveChangesAsync();
        return createdComplexIds;
    }

    private async Task<string> LoginAsync(HttpClient client, string suffix)
    {
        var request = new GoogleLoginRequest { IdToken = $"valid-token-{suffix}" };
        var response = await client.PostAsJsonAsync("/api/v1/auth/google", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
        return result!.AccessToken;
    }
}
