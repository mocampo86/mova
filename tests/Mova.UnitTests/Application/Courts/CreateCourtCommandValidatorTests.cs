using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Validators;

namespace Mova.UnitTests.Application.Courts;

public sealed class CreateCourtCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithMissingFields_IsInvalid()
    {
        var result = await new CreateCourtCommandValidator().ValidateAsync(
            new CreateCourtCommand(Guid.Empty, "", "", "", false, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateCourtCommand.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateCourtCommand.Description));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateCourtCommand.SurfaceType));
    }
}
