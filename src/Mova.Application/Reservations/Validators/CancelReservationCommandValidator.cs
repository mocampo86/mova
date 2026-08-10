using FluentValidation;
using Mova.Application.Reservations.Commands;

namespace Mova.Application.Reservations.Validators;

public sealed class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.CancelledByUserId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
