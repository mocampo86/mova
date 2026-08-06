using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Validators;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateCourtAvailabilityRulesCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithValidData_IsValid()
    {
        var validator = new UpdateCourtAvailabilityRulesCommandValidator();
        var command = new UpdateCourtAvailabilityRulesCommand(Guid.NewGuid(), Guid.NewGuid(),
        [
            new CourtAvailabilityRuleItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true)
        ]);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEndTimeBeforeStartTime_IsInvalid()
    {
        var validator = new UpdateCourtAvailabilityRulesCommandValidator();
        var command = new UpdateCourtAvailabilityRulesCommand(Guid.NewGuid(), Guid.NewGuid(),
        [
            new CourtAvailabilityRuleItem(DayOfWeek.Monday, TimeSpan.FromHours(12), TimeSpan.FromHours(8), 60, true)
        ]);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithSlotDurationNotFitting_IsInvalid()
    {
        var validator = new UpdateCourtAvailabilityRulesCommandValidator();
        var command = new UpdateCourtAvailabilityRulesCommand(Guid.NewGuid(), Guid.NewGuid(),
        [
            new CourtAvailabilityRuleItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 50, true)
        ]);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }
}
