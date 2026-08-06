using FluentValidation;
using Mova.Application.Courts.Commands;

namespace Mova.Application.Courts.Validators;

public sealed class UpdateCourtStatusCommandValidator : AbstractValidator<UpdateCourtStatusCommand>
{
    public UpdateCourtStatusCommandValidator()
    {
        RuleFor(x => x.SportsComplexId)
            .NotEmpty()
            .WithMessage("Sports complex identifier is required.");

        RuleFor(x => x.CourtId)
            .NotEmpty()
            .WithMessage("Court identifier is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User identifier is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid value.");
    }
}
