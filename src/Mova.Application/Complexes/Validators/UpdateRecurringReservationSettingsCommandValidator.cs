using FluentValidation;
using Mova.Application.Complexes.Commands;

namespace Mova.Application.Complexes.Validators;

public sealed class UpdateRecurringReservationSettingsCommandValidator : AbstractValidator<UpdateRecurringReservationSettingsCommand>
{
    public UpdateRecurringReservationSettingsCommandValidator()
    {
        RuleFor(x => x.ComplexId).NotEmpty();
    }
}
