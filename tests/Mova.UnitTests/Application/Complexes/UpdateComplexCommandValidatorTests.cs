using FluentValidation.TestHelper;
using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class UpdateComplexCommandValidatorTests
{
    private readonly UpdateComplexCommandValidator _validator = new();

    private static UpdateComplexCommand CreateValidCommand() =>
        new(
            Guid.NewGuid(),
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com",
            0);

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var command = CreateValidCommand();
        var result = _validator.TestValidate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithComplexIdEmpty_Fails()
    {
        var command = CreateValidCommand() with { ComplexId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ComplexId);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_Fails()
    {
        var longName = new string('a', 256);
        var command = CreateValidCommand() with { Name = longName };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithInvalidLatitude_Fails()
    {
        var command = CreateValidCommand() with { Latitude = 91 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Fact]
    public void Validate_WithInvalidLongitude_Fails()
    {
        var command = CreateValidCommand() with { Longitude = 181 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void Validate_WithInvalidEmail_Fails()
    {
        var command = CreateValidCommand() with { Email = "invalid" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithMissingPhoneNumber_Fails()
    {
        var command = CreateValidCommand() with { PhoneNumber = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithInvalidUtcOffset_Fails()
    {
        var command = CreateValidCommand() with { UtcOffsetMinutes = 1000 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UtcOffsetMinutes);
    }
}
