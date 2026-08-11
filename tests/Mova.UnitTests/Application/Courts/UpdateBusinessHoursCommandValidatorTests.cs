using Mova.Application.Courts.Commands;
using Mova.Application.Courts.Validators;

namespace Mova.UnitTests.Application.Courts;

public sealed class UpdateBusinessHoursCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithValidData_IsValid()
    {
        var validator = new UpdateBusinessHoursCommandValidator();
        var command = new UpdateBusinessHoursCommand(Guid.NewGuid(),
        [
            new BusinessHoursItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false)
        ]);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithOpeningEqualToClosing_IsInvalid()
    {
        var validator = new UpdateBusinessHoursCommandValidator();
        var command = new UpdateBusinessHoursCommand(Guid.NewGuid(),
        [
            new BusinessHoursItem(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(8), false)
        ]);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithClosedDayAndInvalidTimes_IsValid()
    {
        var validator = new UpdateBusinessHoursCommandValidator();
        var command = new UpdateBusinessHoursCommand(Guid.NewGuid(),
        [
            new BusinessHoursItem(DayOfWeek.Sunday, TimeSpan.FromHours(22), TimeSpan.FromHours(8), true)
        ]);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
