using FluentValidation;
using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Validators;

public sealed class CompleteProfileCommandValidator : AbstractValidator<CompleteProfileCommand>
{
    private const string PhoneNumberPattern = @"^\+[0-9](?:\s*[0-9]){6,14}$";
    private const int PhoneNumberMinLength = 8;
    private const int PhoneNumberMaxLength = 50;

    public CompleteProfileCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .MinimumLength(PhoneNumberMinLength)
            .WithMessage($"Phone number must be at least {PhoneNumberMinLength} characters.")
            .MaximumLength(PhoneNumberMaxLength)
            .WithMessage($"Phone number must not exceed {PhoneNumberMaxLength} characters.")
            .Matches(PhoneNumberPattern)
            .WithMessage("Phone number must be in international format starting with '+' followed by digits.");
    }
}
