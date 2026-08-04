using Mova.Application.Authentication.Commands;
using Mova.Application.Authentication.Handlers;
using Mova.Application.Common.Exceptions;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Exceptions;
using Xunit;

namespace Mova.UnitTests.Application.Authentication;

public class GoogleLoginHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeJwtTokenService _jwtTokenService = new();
    private readonly FakeGoogleTokenValidator _googleTokenValidator = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private GoogleLoginHandler CreateHandler() =>
        new(_googleTokenValidator, _jwtTokenService, _userRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithNewUser_CreatesUserAndReturnsToken()
    {
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new GoogleLoginCommand("new-user-token"));

        Assert.NotNull(result);
        Assert.Equal("test-access-token", result.AccessToken);
        Assert.True(result.RequiresProfileCompletion);
        Assert.Equal("user@test.com", result.User.Email);
        Assert.Equal("John Doe", result.User.FullName);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_UpdatesNameAndReturnsToken()
    {
        var existingUser = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "user@test.com", "Old Name");
        await _userRepository.AddAsync(existingUser);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GoogleLoginCommand("any-token"));

        Assert.Equal("John Doe", result.User.FullName);
    }

    [Fact]
    public async Task HandleAsync_WithBlockedUser_ThrowsUserBlockedException()
    {
        var blockedUser = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "user@test.com", "John Doe");
        blockedUser.Block();
        await _userRepository.AddAsync(blockedUser);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UserBlockedException>(() => handler.HandleAsync(new GoogleLoginCommand("any-token")));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidToken_ThrowsAuthenticationException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<AuthenticationException>(() => handler.HandleAsync(new GoogleLoginCommand("invalid-token")));
    }
}
