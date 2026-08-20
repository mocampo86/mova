using FluentValidation.TestHelper;
using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public sealed class UpdateRecurringReservationSettingsCommandValidatorTests
{
    private readonly UpdateRecurringReservationSettingsCommandValidator _validator = new();

    [Fact]
    public void Validate_ComplexIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new UpdateRecurringReservationSettingsCommand(Guid.Empty, true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ComplexId);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new UpdateRecurringReservationSettingsCommand(Guid.NewGuid(), false);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
