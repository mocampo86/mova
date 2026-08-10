using FluentValidation;
using Mova.Application.Authentication.Commands;

namespace Mova.Application.Authentication.Validators;

public sealed class CompleteComplexAdminCommandValidator : AbstractValidator<CompleteComplexAdminCommand>
{
    private const string PhoneNumberPattern = @"^\+[0-9](?:\s*[0-9]){6,14}$";
    private const int PhoneNumberMinLength = 8;
    private const int PhoneNumberMaxLength = 50;
    private const int MaxNameLength = 255;
    private const int MaxDescriptionLength = 2000;
    private const int MaxAddressLength = 255;
    private const int MaxCityLength = 255;
    private const int MaxEmailLength = 255;

    public CompleteComplexAdminCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User identifier is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .MinimumLength(PhoneNumberMinLength)
            .WithMessage($"Phone number must be at least {PhoneNumberMinLength} characters.")
            .MaximumLength(PhoneNumberMaxLength)
            .WithMessage($"Phone number must not exceed {PhoneNumberMaxLength} characters.")
            .Matches(PhoneNumberPattern)
            .WithMessage("Phone number must be in international format starting with '+' followed by digits.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(MaxNameLength)
            .WithMessage($"Name must not exceed {MaxNameLength} characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Description must not exceed {MaxDescriptionLength} characters.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Address is required.")
            .MaximumLength(MaxAddressLength)
            .WithMessage($"Address must not exceed {MaxAddressLength} characters.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(MaxCityLength)
            .WithMessage($"City must not exceed {MaxCityLength} characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m)
            .WithMessage("Latitude must be between -90 and 90.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m)
            .WithMessage("Longitude must be between -180 and 180.")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.ComplexPhoneNumber)
            .NotEmpty()
            .WithMessage("Complex phone number is required.")
            .MinimumLength(PhoneNumberMinLength)
            .WithMessage($"Complex phone number must be at least {PhoneNumberMinLength} characters.")
            .MaximumLength(PhoneNumberMaxLength)
            .WithMessage($"Complex phone number must not exceed {PhoneNumberMaxLength} characters.")
            .Matches(PhoneNumberPattern)
            .WithMessage("Complex phone number must be in international format starting with '+' followed by digits.");

        RuleFor(x => x.ComplexEmail)
            .NotEmpty()
            .WithMessage("Complex email is required.")
            .MaximumLength(MaxEmailLength)
            .WithMessage($"Complex email must not exceed {MaxEmailLength} characters.")
            .EmailAddress()
            .WithMessage("Complex email is not valid.");
    }
}
