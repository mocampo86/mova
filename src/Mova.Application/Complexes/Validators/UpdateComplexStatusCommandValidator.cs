using FluentValidation;
using Mova.Application.Complexes.Commands;

namespace Mova.Application.Complexes.Validators;

public sealed class UpdateComplexStatusCommandValidator : AbstractValidator<UpdateComplexStatusCommand>
{
    public UpdateComplexStatusCommandValidator()
    {
        RuleFor(x => x.ComplexId)
            .NotEmpty()
            .WithMessage("Complex identifier is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User identifier is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid value.");
    }
}
