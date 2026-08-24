using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mova.Api;
using Mova.Application.Abstractions.Persistence;
using Mova.Contracts.Auth;
using Mova.Contracts.Complexes;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;
using Mova.Infrastructure.Persistence.Repositories;
using Mova.IntegrationTests.Authentication;
using Xunit;

namespace Mova.IntegrationTests.Complexes;

[Collection("AuthIntegrationTests")]
public class AtomicOnboardingIntegrationTests : IClassFixture<MovaWebApplicationFactory>
{
    private readonly MovaWebApplicationFactory _factory;

    public AtomicOnboardingIntegrationTests(MovaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateComplex_WithBusinessHoursFailure_RollsBackAndLeavesNoPartialRecords()
    {
        var suffix = $"create-rollback-{Guid.NewGuid()}";
        var factory = CreateFactoryWithFailingBusinessHours(2);
        var client = factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = CreateUniqueCreateRequest(suffix);
        var response = await client.PostAsJsonAsync("/api/v1/complexes", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var user = await context.Users.FirstOrDefaultAsync(u => u.GoogleSubjectId == $"sub-{suffix}");
        Assert.NotNull(user);

        var complex = await context.SportsComplexes.FirstOrDefaultAsync(s => s.Name == request.Name && s.Email == request.Email);
        Assert.Null(complex);

        var admin = await context.ComplexAdministrators.FirstOrDefaultAsync(a => a.UserId == user.Id);
        Assert.Null(admin);

        Assert.False(await context.BusinessHours.AnyAsync(b => b.SportsComplex!.Name == request.Name));
    }

    [Fact]
    public async Task CreateComplex_AfterBusinessHoursFailure_RetrySucceedsAndProducesOneComplex()
    {
        var suffix = $"create-retry-{Guid.NewGuid()}";
        var failingFactory = CreateFactoryWithFailingBusinessHours(0);
        var failingClient = failingFactory.CreateClient();
        var token = await LoginAsync(failingClient, suffix);
        failingClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = CreateUniqueCreateRequest(suffix);
        var failResponse = await failingClient.PostAsJsonAsync("/api/v1/complexes", request);

        Assert.Equal(HttpStatusCode.InternalServerError, failResponse.StatusCode);

        var successClient = _factory.CreateClient();
        successClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var successResponse = await successClient.PostAsJsonAsync("/api/v1/complexes", request);

        successResponse.EnsureSuccessStatusCode();

        var result = await successResponse.Content.ReadFromJsonAsync<SportsComplexInfo>();
        Assert.NotNull(result);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var user = await context.Users.FirstOrDefaultAsync(u => u.GoogleSubjectId == $"sub-{suffix}");
        Assert.NotNull(user);

        var complexes = await context.SportsComplexes
            .Where(s => s.Name == request.Name && s.Email == request.Email)
            .ToListAsync();

        Assert.Single(complexes);
        Assert.Equal(result.Id, complexes[0].Id);

        var admins = await context.ComplexAdministrators
            .Where(a => a.UserId == user.Id && a.SportsComplexId == result.Id)
            .ToListAsync();

        Assert.Single(admins);

        var hours = await context.BusinessHours
            .Where(b => b.SportsComplexId == result.Id)
            .ToListAsync();

        Assert.Equal(7, hours.Count);
    }

    [Fact]
    public async Task CompleteComplexAdmin_WithBusinessHoursFailure_RollsBackAndLeavesNoPartialRecords()
    {
        var suffix = $"complete-rollback-{Guid.NewGuid()}";
        var factory = CreateFactoryWithFailingBusinessHours(2);
        var client = factory.CreateClient();
        var token = await LoginAsync(client, suffix);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = CreateUniqueCompleteRequest(suffix);
        var response = await client.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var user = await context.Users.FirstOrDefaultAsync(u => u.GoogleSubjectId == $"sub-{suffix}");
        Assert.NotNull(user);
        Assert.Null(user.PhoneNumber);
        Assert.DoesNotContain(Role.ComplexAdmin, user.Roles);

        var complex = await context.SportsComplexes.FirstOrDefaultAsync(s => s.Email == request.ComplexEmail);
        Assert.Null(complex);

        var admin = await context.ComplexAdministrators.FirstOrDefaultAsync(a => a.UserId == user.Id);
        Assert.Null(admin);

        Assert.False(await context.BusinessHours.AnyAsync(b => b.SportsComplex!.Email == request.ComplexEmail));
    }

    [Fact]
    public async Task CompleteComplexAdmin_AfterBusinessHoursFailure_RetrySucceedsAndProducesOneComplex()
    {
        var suffix = $"complete-retry-{Guid.NewGuid()}";
        var failingFactory = CreateFactoryWithFailingBusinessHours(0);
        var failingClient = failingFactory.CreateClient();
        var token = await LoginAsync(failingClient, suffix);
        failingClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = CreateUniqueCompleteRequest(suffix);
        var failResponse = await failingClient.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", request);

        Assert.Equal(HttpStatusCode.InternalServerError, failResponse.StatusCode);

        var successClient = _factory.CreateClient();
        successClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var successResponse = await successClient.PostAsJsonAsync("/api/v1/auth/complete-complex-admin", request);

        successResponse.EnsureSuccessStatusCode();

        var result = await successResponse.Content.ReadFromJsonAsync<CompleteComplexAdminResponse>();
        Assert.NotNull(result);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovaDbContext>();

        var user = await context.Users.FirstOrDefaultAsync(u => u.GoogleSubjectId == $"sub-{suffix}");
        Assert.NotNull(user);
        Assert.Equal(request.PhoneNumber, user.PhoneNumber);
        Assert.Contains(Role.ComplexAdmin, user.Roles);

        var complexes = await context.SportsComplexes
            .Where(s => s.Email == request.ComplexEmail)
            .ToListAsync();

        Assert.Single(complexes);
        Assert.Equal(result.ComplexId, complexes[0].Id);

        var admins = await context.ComplexAdministrators
            .Where(a => a.UserId == user.Id && a.SportsComplexId == result.ComplexId)
            .ToListAsync();

        Assert.Single(admins);

        var hours = await context.BusinessHours
            .Where(b => b.SportsComplexId == result.ComplexId)
            .ToListAsync();

        Assert.Equal(7, hours.Count);
    }

    private WebApplicationFactory<Program> CreateFactoryWithFailingBusinessHours(int failAfter)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IBusinessHoursRepository>(sp =>
                {
                    var context = sp.GetRequiredService<MovaDbContext>();
                    var inner = new BusinessHoursRepository(context);
                    return new FailingBusinessHoursRepository(inner, failAfter);
                });
            });
        });
    }

    private static CreateComplexRequest CreateUniqueCreateRequest(string suffix) =>
        new()
        {
            Name = $"Club {suffix}",
            Description = "A premium padel club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            Latitude = -34.6m,
            Longitude = -58.3m,
            PhoneNumber = "+54 11 1234 5678",
            Email = $"contact-{suffix}@clubpadel.com",
            TimeZoneId = "America/Argentina/Buenos_Aires"
        };

    private static CompleteComplexAdminRequest CreateUniqueCompleteRequest(string suffix) =>
        new()
        {
            PhoneNumber = "+54 11 1234 5678",
            Name = $"Club {suffix}",
            Description = "A premium padel club",
            Address = "Av. Libertador 1234",
            City = "Buenos Aires",
            Latitude = -34.6m,
            Longitude = -58.3m,
            ComplexPhoneNumber = "+54 11 1234 5678",
            ComplexEmail = $"contact-{suffix}@clubpadel.com",
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
