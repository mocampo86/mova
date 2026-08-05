using FluentValidation.TestHelper;
using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Validators;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class UpdateComplexStatusCommandValidatorTests
{
    private readonly UpdateComplexStatusCommandValidator _validator = new();

    private static UpdateComplexStatusCommand CreateValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), ComplexStatus.Active);

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
    public void Validate_WithUserIdEmpty_Fails()
    {
        var command = CreateValidCommand() with { UserId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithInvalidStatus_Fails()
    {
        var command = CreateValidCommand() with { Status = (ComplexStatus)99 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
