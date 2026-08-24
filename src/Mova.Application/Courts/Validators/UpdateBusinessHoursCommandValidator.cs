using FluentValidation;
using Mova.Application.Courts.Commands;

namespace Mova.Application.Courts.Validators;

public sealed class UpdateBusinessHoursCommandValidator : AbstractValidator<UpdateBusinessHoursCommand>
{
    private static readonly TimeSpan Day = TimeSpan.FromHours(24);

    public UpdateBusinessHoursCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleForEach(x => x.Hours).ChildRules(hours =>
        {
            hours.RuleFor(x => x.DayOfWeek).Must(BeValidDayOfWeek)
                .WithMessage("DayOfWeek must be between 0 and 6.");
            hours.RuleFor(x => x.OpeningTime).Must(BeValidTimeOfDay)
                .WithMessage("Opening time must be within a 24-hour day.");
            hours.RuleFor(x => x.ClosingTime).Must(BeValidTimeOfDay)
                .WithMessage("Closing time must be within a 24-hour day.");
            hours.RuleFor(x => x).Must(x => x.IsClosed || x.OpeningTime != x.ClosingTime)
                .WithMessage("Opening and closing times cannot be the same when the day is not closed.");
        });
    }

    private static bool BeValidDayOfWeek(DayOfWeek dayOfWeek)
    {
        var value = (int)dayOfWeek;
        return value >= 0 && value <= 6;
    }

    private static bool BeValidTimeOfDay(TimeSpan time)
    {
        return time >= TimeSpan.Zero && time < Day;
    }
}
