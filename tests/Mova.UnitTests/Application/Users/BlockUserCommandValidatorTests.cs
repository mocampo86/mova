using Mova.Application.Users.Commands;
using Mova.Application.Users.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class BlockUserCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithMissingFields_IsInvalid()
    {
        var result = await new BlockUserCommandValidator().ValidateAsync(
            new BlockUserCommand(Guid.Empty, Guid.Empty, Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(BlockUserCommand.SportsComplexId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(BlockUserCommand.UserId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(BlockUserCommand.BlockedByUserId));
    }

    [Fact]
    public async Task Validate_WithBlockedUntilInPast_IsInvalid()
    {
        var command = new BlockUserCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason",
            DateTime.UtcNow.AddDays(-1));

        var result = await new BlockUserCommandValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(BlockUserCommand.BlockedUntil));
    }

    [Fact]
    public async Task Validate_WithValidData_IsValid()
    {
        var command = new BlockUserCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason",
            DateTime.UtcNow.AddDays(1));

        var result = await new BlockUserCommandValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
