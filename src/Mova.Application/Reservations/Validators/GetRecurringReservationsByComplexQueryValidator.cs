using FluentValidation;
using Mova.Application.Reservations.Queries;

namespace Mova.Application.Reservations.Validators;

public sealed class GetRecurringReservationsByComplexQueryValidator : AbstractValidator<GetRecurringReservationsByComplexQuery>
{
    public GetRecurringReservationsByComplexQueryValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
