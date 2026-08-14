using FluentValidation;
using Mova.Application.Reservations.Queries;

namespace Mova.Application.Reservations.Validators;

public sealed class GetCancellationPolicyQueryValidator : AbstractValidator<GetCancellationPolicyQuery>
{
    public GetCancellationPolicyQueryValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
    }
}
