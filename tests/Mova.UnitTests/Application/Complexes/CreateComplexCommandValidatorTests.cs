using FluentValidation.TestHelper;
using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class CreateComplexCommandValidatorTests
{
    private readonly CreateComplexCommandValidator _validator = new();

    private static CreateComplexCommand CreateValidCommand() =>
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
            "America/Montevideo");

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var command = CreateValidCommand();
        var result = _validator.TestValidate(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithMissingName_Fails(string name)
    {
        var command = CreateValidCommand() with { Name = name };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_Fails()
    {
        var longName = new string('a', 256);
        var command = CreateValidCommand() with { Name = longName };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithMissingDescription_Fails(string description)
    {
        var command = CreateValidCommand() with { Description = description };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithMissingAddress_Fails(string address)
    {
        var command = CreateValidCommand() with { Address = address };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithMissingCity_Fails(string city)
    {
        var command = CreateValidCommand() with { City = city };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_WithInvalidLatitude_Fails(decimal latitude)
    {
        var command = CreateValidCommand() with { Latitude = latitude };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_WithInvalidLongitude_Fails(decimal longitude)
    {
        var command = CreateValidCommand() with { Longitude = longitude };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("+123")]
    [InlineData("123456789")]
    [InlineData("abc")]
    public void Validate_WithInvalidPhoneNumber_Fails(string phoneNumber)
    {
        var command = CreateValidCommand() with { PhoneNumber = phoneNumber };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    public void Validate_WithInvalidEmail_Fails(string email)
    {
        var command = CreateValidCommand() with { Email = email };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithUserIdEmpty_Fails()
    {
        var command = CreateValidCommand() with { UserId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithMissingTimeZoneId_Fails(string timeZoneId)
    {
        var command = CreateValidCommand() with { TimeZoneId = timeZoneId };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TimeZoneId);
    }

    [Fact]
    public void Validate_WithInvalidTimeZoneId_Fails()
    {
        var command = CreateValidCommand() with { TimeZoneId = "Not/A/TimeZone" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TimeZoneId);
    }
}
