using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Mova.Api;
using Mova.Contracts.Errors;
using Xunit;

namespace Mova.IntegrationTests.Exceptions;

public class GlobalExceptionHandlerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GlobalExceptionHandlerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Production_Environment_ReturnsGenericErrorResponse()
    {
        var client = _factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Production"))
            .CreateClient();

        var response = await client.GetAsync("/api/test/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ApiErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(error);
        Assert.Equal("INTERNAL_SERVER_ERROR", error!.Error.Code);
        Assert.Equal("An unexpected error occurred. Please try again later.", error.Error.Message);
        Assert.Null(error.Error.Details);
        Assert.False(string.IsNullOrWhiteSpace(error.Error.TraceId));
    }

    [Fact]
    public async Task Development_Environment_ReturnsDetailedErrorResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/test/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ApiErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(error);
        Assert.Equal("INTERNAL_SERVER_ERROR", error!.Error.Code);
        Assert.Contains("Test exception", error.Error.Message);
        Assert.NotNull(error.Error.Details);
        Assert.True(error.Error.Details!.ContainsKey("stackTrace"));
    }
}
