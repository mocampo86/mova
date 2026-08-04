using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public class CompleteProfileHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CompleteProfileHandler CreateHandler() =>
        new(_userRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithValidPhoneNumber_CompletesProfileAndReturnsUserInfo()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "user@test.com", "John Doe");
        await _userRepository.AddAsync(user);
        var handler = CreateHandler();

        var result = await handler.HandleAsync(new CompleteProfileCommand(user.Id, "+54 11 1234 5678"));

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("user@test.com", result.Email);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("+54 11 1234 5678", result.PhoneNumber);
        Assert.False(result.PhoneVerified);
        Assert.Equal("+54 11 1234 5678", user.PhoneNumber);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(new CompleteProfileCommand(Guid.NewGuid(), "+54 11 1234 5678")));
    }
}
