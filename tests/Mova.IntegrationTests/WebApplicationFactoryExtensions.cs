using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mova.Api;
using Mova.Api.Configuration;

namespace Mova.IntegrationTests;

public static class WebApplicationFactoryExtensions
{
    public static WebApplicationFactory<Program> WithRateLimits(
        this WebApplicationFactory<Program> factory,
        RateLimitingPolicyOptions? search = null,
        RateLimitingPolicyOptions? login = null,
        RateLimitingPolicyOptions? reservation = null)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.Configure<RateLimitingOptions>(options =>
                {
                    options.Enabled = true;

                    if (search is not null)
                    {
                        options.Search = search;
                    }

                    if (login is not null)
                    {
                        options.Login = login;
                    }

                    if (reservation is not null)
                    {
                        options.Reservation = reservation;
                    }
                });
            });
        });
    }
}
