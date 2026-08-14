using FluentValidation;
using Mova.Application.Reservations.Commands;

namespace Mova.Application.Reservations.Validators;

public sealed class CancelRecurringReservationCommandValidator : AbstractValidator<CancelRecurringReservationCommand>
{
    public CancelRecurringReservationCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.RecurringReservationId).NotEmpty();
        RuleFor(x => x.CancelledByUserId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(2000);
    }
}
