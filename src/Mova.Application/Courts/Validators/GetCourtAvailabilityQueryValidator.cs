using FluentValidation;
using Mova.Application.Courts.Queries;

namespace Mova.Application.Courts.Validators;

public sealed class GetCourtAvailabilityQueryValidator : AbstractValidator<GetCourtAvailabilityQuery>
{
    public GetCourtAvailabilityQueryValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
    }
}
