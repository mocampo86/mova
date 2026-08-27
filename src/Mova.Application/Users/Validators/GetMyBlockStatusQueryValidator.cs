using FluentValidation;
using Mova.Application.Users.Queries;

namespace Mova.Application.Users.Validators;

public sealed class GetMyBlockStatusQueryValidator : AbstractValidator<GetMyBlockStatusQuery>
{
    public GetMyBlockStatusQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ComplexId).NotEmpty();
    }
}
