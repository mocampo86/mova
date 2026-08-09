using FluentValidation.TestHelper;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateCourtCommandValidatorTests
{
    private readonly UpdateCourtCommandValidator _validator = new();

    private static UpdateCourtCommand CreateValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Court", "Description", "Synthetic", false, null);

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var result = _validator.TestValidate(CreateValidCommand());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptySportsComplexId_Fails()
    {
        var command = CreateValidCommand() with { SportsComplexId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SportsComplexId);
    }

    [Fact]
    public void Validate_WithEmptyCourtId_Fails()
    {
        var command = CreateValidCommand() with { CourtId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CourtId);
    }

    [Fact]
    public void Validate_WithEmptyName_Fails()
    {
        var command = CreateValidCommand() with { Name = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameTooLong_Fails()
    {
        var command = CreateValidCommand() with { Name = new string('a', 256) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithEmptyDescription_Fails()
    {
        var command = CreateValidCommand() with { Description = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_Fails()
    {
        var command = CreateValidCommand() with { Description = new string('a', 2001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithEmptySurfaceType_Fails()
    {
        var command = CreateValidCommand() with { SurfaceType = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SurfaceType);
    }

    [Fact]
    public void Validate_WithSurfaceTypeTooLong_Fails()
    {
        var command = CreateValidCommand() with { SurfaceType = new string('a', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SurfaceType);
    }

    [Fact]
    public void Validate_WithEmptySportId_Fails()
    {
        var command = CreateValidCommand() with { SportIds = [Guid.Empty] };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("SportIds[0]");
    }
}
