using FluentValidation;
using Mova.Application.Reservations.Commands;

namespace Mova.Application.Reservations.Validators;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.StartAt).NotEmpty();
        RuleFor(x => x.EndAt).NotEmpty().GreaterThan(x => x.StartAt);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
