using FluentValidation.TestHelper;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public class CompleteProfileCommandValidatorTests
{
    private readonly CompleteProfileCommandValidator _validator = new();

    [Theory]
    [InlineData("+541112345678")]
    [InlineData("+54 11 1234 5678")]
    [InlineData("+1 555 555 5555")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("+299 12 34 56")]
    public void Validate_WithValidPhoneNumber_Passes(string phoneNumber)
    {
        var command = new CompleteProfileCommand(Guid.NewGuid(), phoneNumber);
        var result = _validator.TestValidate(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("+123")]
    [InlineData("123456789")]
    [InlineData("abc")]
    [InlineData("+abc")]
    [InlineData("+54 11 abc")]
    public void Validate_WithInvalidPhoneNumber_Fails(string phoneNumber)
    {
        var command = new CompleteProfileCommand(Guid.NewGuid(), phoneNumber);
        var result = _validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithPhoneNumberExceedingMaxLength_Fails()
    {
        var longPhoneNumber = "+" + new string('1', 60);
        var command = new CompleteProfileCommand(Guid.NewGuid(), longPhoneNumber);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
