using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Mova.Api.RateLimiting;
using Xunit;

namespace Mova.UnitTests.Api.RateLimiting;

public class RateLimitingPartitionKeyResolverTests
{
    [Fact]
    public void ResolveLoginPartitionKey_WithRemoteIpAddress_ReturnsIpPrefixedKey()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.5");

        var key = RateLimitingPartitionKeyResolver.ResolveLoginPartitionKey(context);

        Assert.Equal("ip:203.0.113.5", key);
    }

    [Fact]
    public void ResolveLoginPartitionKey_WithAuthenticatedUser_ReturnsClientIp_NotUserId()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("sub", Guid.NewGuid().ToString())
        ], "Bearer"));
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

        var key = RateLimitingPartitionKeyResolver.ResolveLoginPartitionKey(context);

        Assert.Equal("ip:203.0.113.10", key);
    }

    [Fact]
    public void ResolveLoginPartitionKey_WithoutRemoteIpAddress_ReturnsAnonymous()
    {
        var context = new DefaultHttpContext();

        var key = RateLimitingPartitionKeyResolver.ResolveLoginPartitionKey(context);

        Assert.Equal("anonymous", key);
    }

    [Fact]
    public void ResolveAuthenticatedPartitionKey_WithAuthenticatedUser_ReturnsUserPrefixedKeyFromSubClaim()
    {
        var userId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("sub", userId)
        ], "Bearer"));
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.5");

        var key = RateLimitingPartitionKeyResolver.ResolveAuthenticatedPartitionKey(context);

        Assert.Equal($"user:{userId}", key);
    }

    [Fact]
    public void ResolveAuthenticatedPartitionKey_WithAuthenticatedUser_ReturnsUserPrefixedKeyFromNameIdentifierClaim()
    {
        var userId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "Bearer"));

        var key = RateLimitingPartitionKeyResolver.ResolveAuthenticatedPartitionKey(context);

        Assert.Equal($"user:{userId}", key);
    }

    [Fact]
    public void ResolveAuthenticatedPartitionKey_WithoutAuthenticatedUser_FallsBackToClientIp()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");

        var key = RateLimitingPartitionKeyResolver.ResolveAuthenticatedPartitionKey(context);

        Assert.Equal("ip:203.0.113.7", key);
    }

    [Fact]
    public void ResolveAuthenticatedPartitionKey_WithoutUserAndRemoteIpAddress_ReturnsAnonymous()
    {
        var context = new DefaultHttpContext();

        var key = RateLimitingPartitionKeyResolver.ResolveAuthenticatedPartitionKey(context);

        Assert.Equal("anonymous", key);
    }
}
