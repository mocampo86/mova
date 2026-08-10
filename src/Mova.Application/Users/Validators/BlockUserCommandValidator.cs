using FluentValidation;
using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Validators;

public sealed class BlockUserCommandValidator : AbstractValidator<BlockUserCommand>
{
    public BlockUserCommandValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BlockedByUserId).NotEmpty();
        RuleFor(x => x.BlockedUntil)
            .Must(BeInTheFutureWhenProvided)
            .When(x => x.BlockedUntil.HasValue)
            .WithMessage("BlockedUntil must be in the future.");
    }

    private static bool BeInTheFutureWhenProvided(DateTime? blockedUntil)
    {
        return blockedUntil.HasValue && blockedUntil.Value > DateTime.UtcNow;
    }
}
