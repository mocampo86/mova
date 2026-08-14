using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class UpdateCancellationPolicyCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithMissingFields_IsInvalid()
    {
        var result = await new UpdateCancellationPolicyCommandValidator().ValidateAsync(
            new UpdateCancellationPolicyCommand(Guid.Empty, -1, true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateCancellationPolicyCommand.SportsComplexId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateCancellationPolicyCommand.MinimumHours));
    }

    [Fact]
    public async Task Validate_WithNegativeMinimumHours_IsInvalid()
    {
        var command = new UpdateCancellationPolicyCommand(Guid.NewGuid(), -5, true);

        var result = await new UpdateCancellationPolicyCommandValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateCancellationPolicyCommand.MinimumHours));
    }

    [Fact]
    public async Task Validate_WithValidData_IsValid()
    {
        var command = new UpdateCancellationPolicyCommand(Guid.NewGuid(), 0, false);

        var result = await new UpdateCancellationPolicyCommandValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithFractionalNotApplicable_IntegerIsValid()
    {
        var command = new UpdateCancellationPolicyCommand(Guid.NewGuid(), 12, true);

        var result = await new UpdateCancellationPolicyCommandValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
