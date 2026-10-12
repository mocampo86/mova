using FluentValidation;
using Mova.Application.CourtRates.Queries;

namespace Mova.Application.CourtRates.Validators;

public sealed class GetPriceQuoteQueryValidator : AbstractValidator<GetPriceQuoteQuery>
{
    public GetPriceQuoteQueryValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.StartAt).NotEmpty();
        RuleFor(x => x.EndAt).NotEmpty().GreaterThan(x => x.StartAt);
    }
}
