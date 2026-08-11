using FluentValidation;
using Mova.Application.Users.Queries;

namespace Mova.Application.Users.Validators;

public sealed class GetUserDashboardQueryValidator : AbstractValidator<GetUserDashboardQuery>
{
    public GetUserDashboardQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UpcomingPage).GreaterThanOrEqualTo(1);
        RuleFor(x => x.UpcomingPageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.HistoryPageSize).InclusiveBetween(1, 100);
    }
}
