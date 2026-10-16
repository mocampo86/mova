using FluentValidation;
using Mova.Application.Users.Queries;

namespace Mova.Application.Users.Validators;

public sealed class GetMyFavoriteComplexesQueryValidator : AbstractValidator<GetMyFavoriteComplexesQuery>
{
    public GetMyFavoriteComplexesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
