using FluentValidation;
using Mova.Application.Courts.Commands;

namespace Mova.Application.Courts.Validators;

public sealed class AssignCourtSportsCommandValidator : AbstractValidator<AssignCourtSportsCommand>
{
    public AssignCourtSportsCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.SportIds).NotNull().Must(x => x.Count > 0)
            .WithMessage("At least one sport must be assigned to the court.");
        RuleForEach(x => x.SportIds).NotEmpty();
    }
}
