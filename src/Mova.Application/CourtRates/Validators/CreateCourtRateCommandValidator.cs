using FluentValidation;
using Mova.Application.CourtRates.Commands;

namespace Mova.Application.CourtRates.Validators;

public sealed class CreateCourtRateCommandValidator : AbstractValidator<CreateCourtRateCommand>
{
    public CreateCourtRateCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.DaysOfWeek).ValidDaysOfWeek();
        RuleFor(x => x.StartTime).ValidRateStartTime();
        RuleFor(x => x.EndTime).ValidRateEndTime(x => x.StartTime);
        RuleFor(x => x.AmountPerHour).ValidAmountPerHour();
    }
}
