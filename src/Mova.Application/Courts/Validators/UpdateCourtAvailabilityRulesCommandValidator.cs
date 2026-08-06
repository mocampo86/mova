using FluentValidation;
using Mova.Application.Courts.Commands;

namespace Mova.Application.Courts.Validators;

public sealed class UpdateCourtAvailabilityRulesCommandValidator : AbstractValidator<UpdateCourtAvailabilityRulesCommand>
{
    public UpdateCourtAvailabilityRulesCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleForEach(x => x.Rules).ChildRules(rule =>
        {
            rule.RuleFor(x => x.SlotDurationMinutes).GreaterThan(0);
            rule.RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
            rule.RuleFor(x => x)
                .Must(x => FitsSlotDuration(x.StartTime, x.EndTime, x.SlotDurationMinutes))
                .WithMessage("The time range must be evenly divisible by the slot duration.");
        });
    }

    private static bool FitsSlotDuration(TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes)
    {
        var duration = endTime - startTime;
        return duration.TotalMinutes % slotDurationMinutes == 0;
    }
}
