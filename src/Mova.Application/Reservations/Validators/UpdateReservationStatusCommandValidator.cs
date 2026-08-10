using FluentValidation;
using Mova.Application.Reservations.Commands;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Validators;

public sealed class UpdateReservationStatusCommandValidator : AbstractValidator<UpdateReservationStatusCommand>
{
    public UpdateReservationStatusCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Status)
            .Must(status => status is ReservationStatus.Completed or ReservationStatus.NoShow)
            .WithMessage("Status must be Completed or NoShow.");
    }
}
