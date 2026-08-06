using FluentValidation.TestHelper;
using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Validators;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateCourtStatusCommandValidatorTests
{
    private readonly UpdateCourtStatusCommandValidator _validator = new();

    private static UpdateCourtStatusCommand CreateValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CourtStatus.Active);

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var command = CreateValidCommand();
        var result = _validator.TestValidate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithSportsComplexIdEmpty_Fails()
    {
        var command = CreateValidCommand() with { SportsComplexId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SportsComplexId);
    }

    [Fact]
    public void Validate_WithCourtIdEmpty_Fails()
    {
        var command = CreateValidCommand() with { CourtId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CourtId);
    }

    [Fact]
    public void Validate_WithUserIdEmpty_Fails()
    {
        var command = CreateValidCommand() with { UserId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithInvalidStatus_Fails()
    {
        var command = CreateValidCommand() with { Status = (CourtStatus)99 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
