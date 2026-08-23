using FluentValidation;
using Mova.Application.Courts.Commands;
using Mova.Domain.Entities;

namespace Mova.Application.Courts.Validators;

public sealed class UpdateCourtAvailabilityRulesCommandValidator : AbstractValidator<UpdateCourtAvailabilityRulesCommand>
{
    public UpdateCourtAvailabilityRulesCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleForEach(x => x.Rules).ChildRules(rule =>
        {
            rule.RuleFor(x => x.DayOfWeek).IsInEnum();
            rule.RuleFor(x => x.SlotDurationMinutes).GreaterThan(0);
            rule.RuleFor(x => x.StartTime).Must(BeValidTimeOfDay)
                .WithMessage("Start time must be within a 24-hour day.");
            rule.RuleFor(x => x.EndTime).Must(BeValidTimeOfDay)
                .WithMessage("End time must be within a 24-hour day.");
            rule.RuleFor(x => x.EndTime).NotEqual(x => x.StartTime);
            rule.RuleFor(x => x)
                .Must(x => CourtAvailabilityRule.FitsSlotDuration(x.StartTime, x.EndTime, x.SlotDurationMinutes))
                .WithMessage("The time range must be evenly divisible by the slot duration.")
                .When(x => x.StartTime != x.EndTime);
        });
    }

    private static bool BeValidTimeOfDay(TimeSpan time)
    {
        return time >= TimeSpan.Zero && time < TimeSpan.FromHours(24);
    }
}
