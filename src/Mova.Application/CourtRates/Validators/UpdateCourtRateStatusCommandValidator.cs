using FluentValidation;
using Mova.Application.CourtRates.Commands;

namespace Mova.Application.CourtRates.Validators;

public sealed class UpdateCourtRateStatusCommandValidator : AbstractValidator<UpdateCourtRateStatusCommand>
{
    public UpdateCourtRateStatusCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.RateId).NotEmpty();
    }
}
