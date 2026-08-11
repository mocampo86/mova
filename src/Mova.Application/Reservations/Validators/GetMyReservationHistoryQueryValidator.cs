using FluentValidation;
using Mova.Application.Reservations.Queries;

namespace Mova.Application.Reservations.Validators;

public sealed class GetMyReservationHistoryQueryValidator : AbstractValidator<GetMyReservationHistoryQuery>
{
    public GetMyReservationHistoryQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
