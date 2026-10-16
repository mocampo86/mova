using FluentValidation;
using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Validators;

public sealed class AddFavoriteComplexCommandValidator : AbstractValidator<AddFavoriteComplexCommand>
{
    public AddFavoriteComplexCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SportsComplexId).NotEmpty();
    }
}
