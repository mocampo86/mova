using FluentValidation;
using Mova.Application.Authentication.Commands;

namespace Mova.Application.Authentication.Validators;

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("IdToken is required.");
    }
}
