using FluentValidation;
using Mova.Application.Users.Queries;

namespace Mova.Application.Users.Validators;

public sealed class GetUsersByComplexQueryValidator : AbstractValidator<GetUsersByComplexQuery>
{
    public GetUsersByComplexQueryValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
