using FluentValidation;
using Mova.Application.Courts.Commands;

namespace Mova.Application.Courts.Validators;

public sealed class UpdateCourtCommandValidator : AbstractValidator<UpdateCourtCommand>
{
    public UpdateCourtCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.SurfaceType).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.SportIds).NotEmpty();
    }
}
