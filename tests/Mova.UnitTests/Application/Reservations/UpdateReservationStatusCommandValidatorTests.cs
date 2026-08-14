using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Validators;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class UpdateReservationStatusCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithMissingFields_IsInvalid()
    {
        var result = await new UpdateReservationStatusCommandValidator().ValidateAsync(
            new UpdateReservationStatusCommand(Guid.Empty, Guid.Empty, default));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateReservationStatusCommand.SportsComplexId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateReservationStatusCommand.ReservationId));
    }

    [Fact]
    public async Task Validate_WithStatusNotCompletedOrNoShow_IsInvalid()
    {
        var result = await new UpdateReservationStatusCommandValidator().ValidateAsync(
            new UpdateReservationStatusCommand(Guid.NewGuid(), Guid.NewGuid(), ReservationStatus.Confirmed));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateReservationStatusCommand.Status));
    }

    [Fact]
    public async Task Validate_WithCompletedStatus_IsValid()
    {
        var result = await new UpdateReservationStatusCommandValidator().ValidateAsync(
            new UpdateReservationStatusCommand(Guid.NewGuid(), Guid.NewGuid(), ReservationStatus.Completed));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithNoShowStatus_IsValid()
    {
        var result = await new UpdateReservationStatusCommandValidator().ValidateAsync(
            new UpdateReservationStatusCommand(Guid.NewGuid(), Guid.NewGuid(), ReservationStatus.NoShow));

        Assert.True(result.IsValid);
    }
}
