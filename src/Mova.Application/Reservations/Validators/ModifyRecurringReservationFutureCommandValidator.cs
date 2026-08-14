using FluentValidation;
using Mova.Application.Reservations.Commands;

namespace Mova.Application.Reservations.Validators;

public sealed class ModifyRecurringReservationFutureCommandValidator : AbstractValidator<ModifyRecurringReservationFutureCommand>
{
    private const int MaximumWeeks = 52;

    public ModifyRecurringReservationFutureCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.RecurringReservationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DayOfWeek).IsInEnum();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 24 * 60);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.EffectiveDate);
        RuleFor(x => x.EndDate.DayNumber - x.EffectiveDate.DayNumber)
            .LessThanOrEqualTo(MaximumWeeks * 7)
            .WithMessage($"Recurring reservation changes cannot exceed {MaximumWeeks} weeks.");
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
