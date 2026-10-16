using FluentValidation;
using Mova.Application.Users.Queries;

namespace Mova.Application.Users.Validators;

public sealed class GetMyFavoriteComplexIdsQueryValidator : AbstractValidator<GetMyFavoriteComplexIdsQuery>
{
    public GetMyFavoriteComplexIdsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
