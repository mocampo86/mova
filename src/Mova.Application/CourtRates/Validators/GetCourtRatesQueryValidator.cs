using FluentValidation;
using Mova.Application.CourtRates.Queries;

namespace Mova.Application.CourtRates.Validators;

public sealed class GetCourtRatesQueryValidator : AbstractValidator<GetCourtRatesQuery>
{
    public GetCourtRatesQueryValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
    }
}
