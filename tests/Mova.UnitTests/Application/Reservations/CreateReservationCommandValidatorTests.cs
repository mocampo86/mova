using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class CreateReservationCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithMissingFields_IsInvalid()
    {
        var result = await new CreateReservationCommandValidator().ValidateAsync(
            new CreateReservationCommand(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, default, default, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateReservationCommand.SportsComplexId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateReservationCommand.CourtId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateReservationCommand.UserId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateReservationCommand.CreatedByUserId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateReservationCommand.StartAt));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateReservationCommand.EndAt));
    }

    [Fact]
    public async Task Validate_WithEndBeforeStart_IsInvalid()
    {
        var start = new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(-1);

        var result = await new CreateReservationCommandValidator().ValidateAsync(
            new CreateReservationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateReservationCommand.EndAt));
    }
}
