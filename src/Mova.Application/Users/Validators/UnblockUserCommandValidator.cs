using FluentValidation;
using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Validators;

public sealed class UnblockUserCommandValidator : AbstractValidator<UnblockUserCommand>
{
    public UnblockUserCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.BlockedUserId).NotEmpty();
    }
}
