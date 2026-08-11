using FluentValidation;
using Mova.Application.Courts.Commands;

namespace Mova.Application.Courts.Validators;

public sealed class UpdateBusinessHoursCommandValidator : AbstractValidator<UpdateBusinessHoursCommand>
{
    public UpdateBusinessHoursCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleForEach(x => x.Hours).ChildRules(hours =>
        {
            hours.RuleFor(x => x).Must(x => x.IsClosed || x.OpeningTime != x.ClosingTime)
                .WithMessage("Opening and closing times cannot be the same when the day is not closed.");
        });
    }
}
