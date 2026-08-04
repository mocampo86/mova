using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mova.Domain.Entities;
using Mova.Infrastructure.Authentication;
using Mova.Infrastructure.Authentication.Options;
using Xunit;

namespace Mova.UnitTests.Infrastructure.Authentication;

public class JwtTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        SecretKey = "test-secret-must-be-at-least-32-bytes-long!",
        Issuer = "Mova",
        Audience = "Mova.Api",
        ExpirationMinutes = 15
    };

    [Fact]
    public async Task GenerateAsync_WithValidUser_ReturnsTokenWithRequiredClaims()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "user@test.com", "John Doe");
        var service = new JwtTokenService(Options.Create(_options));

        var token = await service.GenerateAsync(user, ["User"]);

        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(token.AccessToken, validationParameters, out var validatedToken);
        var jwt = (JwtSecurityToken)validatedToken;

        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal("user@test.com", jwt.Claims.Single(c => c.Type == "email").Value);
        Assert.Equal("John Doe", jwt.Claims.Single(c => c.Type == "name").Value);
        Assert.Contains(jwt.Claims, c => c.Type == "roles" && c.Value == "User");
        Assert.Equal("Mova", jwt.Issuer);
        Assert.Equal("Mova.Api", jwt.Audiences.Single());
        Assert.NotNull(principal);
    }

    [Fact]
    public async Task GenerateAsync_WithShortSecretKey_ThrowsInvalidOperationException()
    {
        var options = Options.Create(new JwtOptions { SecretKey = "short" });
        var service = new JwtTokenService(options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateAsync(
            User.CreateFromGoogle(Guid.NewGuid(), "sub", "user@test.com", "John Doe"),
            ["User"]));
    }
}
