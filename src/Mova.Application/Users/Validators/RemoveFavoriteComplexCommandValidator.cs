using FluentValidation;
using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Validators;

public sealed class RemoveFavoriteComplexCommandValidator : AbstractValidator<RemoveFavoriteComplexCommand>
{
    public RemoveFavoriteComplexCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SportsComplexId).NotEmpty();
    }
}
