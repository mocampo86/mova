using FluentValidation;
using Mova.Application.Reservations.Commands;

namespace Mova.Application.Reservations.Validators;

public sealed class CreateRecurringReservationCommandValidator : AbstractValidator<CreateRecurringReservationCommand>
{
    private const int MaximumWeeks = 52;

    public CreateRecurringReservationCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DayOfWeek).IsInEnum();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 24 * 60);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.EndDate.DayNumber - x.StartDate.DayNumber)
            .LessThanOrEqualTo(MaximumWeeks * 7)
            .WithMessage($"Recurring reservations cannot exceed {MaximumWeeks} weeks.");
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
