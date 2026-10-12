using FluentValidation;

namespace Mova.Application.CourtRates.Validators;

internal static class CourtRateRulesValidatorExtensions
{
    private static readonly TimeSpan Day = TimeSpan.FromHours(24);

    public static IRuleBuilderOptions<T, IReadOnlyCollection<DayOfWeek>> ValidDaysOfWeek<T>(
        this IRuleBuilder<T, IReadOnlyCollection<DayOfWeek>> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("At least one day of week must be selected.")
            .Must(days => days.All(d => d is >= DayOfWeek.Sunday and <= DayOfWeek.Saturday))
            .WithMessage("Days of week must be between 0 (Sunday) and 6 (Saturday).");
    }

    public static IRuleBuilderOptions<T, TimeSpan> ValidRateStartTime<T>(
        this IRuleBuilder<T, TimeSpan> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(TimeSpan.Zero).WithMessage("StartTime must be within a 24-hour day.")
            .LessThan(Day).WithMessage("StartTime must be within a 24-hour day.");
    }

    public static IRuleBuilderOptions<T, TimeSpan> ValidRateEndTime<T>(
        this IRuleBuilder<T, TimeSpan> ruleBuilder,
        Func<T, TimeSpan> startTime)
    {
        return ruleBuilder
            .Must((instance, endTime) => endTime > startTime(instance)).WithMessage("EndTime must be greater than StartTime.")
            .LessThanOrEqualTo(Day).WithMessage("EndTime must be at most 24:00.");
    }

    public static IRuleBuilderOptions<T, decimal> ValidAmountPerHour<T>(
        this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Amount per hour must be greater than zero.")
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("Amount per hour must have at most two decimal places.");
    }
}
