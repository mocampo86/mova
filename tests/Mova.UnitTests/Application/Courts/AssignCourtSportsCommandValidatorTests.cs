using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Validators;

namespace Mova.UnitTests.Application.Courts;

public sealed class AssignCourtSportsCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithoutSports_IsInvalid()
    {
        var result = await new AssignCourtSportsCommandValidator().ValidateAsync(
            new AssignCourtSportsCommand(Guid.NewGuid(), Guid.NewGuid(), []));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(AssignCourtSportsCommand.SportIds));
    }
}
