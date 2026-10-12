using FluentValidation;
using Mova.Application.CourtRates.Commands;

namespace Mova.Application.CourtRates.Validators;

public sealed class UpdateCourtRateCommandValidator : AbstractValidator<UpdateCourtRateCommand>
{
    public UpdateCourtRateCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.RateId).NotEmpty();
        RuleFor(x => x.DaysOfWeek).ValidDaysOfWeek();
        RuleFor(x => x.StartTime).ValidRateStartTime();
        RuleFor(x => x.EndTime).ValidRateEndTime(x => x.StartTime);
        RuleFor(x => x.AmountPerHour).ValidAmountPerHour();
    }
}
