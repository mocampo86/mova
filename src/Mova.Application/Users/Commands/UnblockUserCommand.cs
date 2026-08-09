namespace Mova.Application.Users.Commands;

public sealed class UnblockUserCommand
{
    public UnblockUserCommand(Guid sportsComplexId, Guid blockedUserId)
    {
        SportsComplexId = sportsComplexId;
        BlockedUserId = blockedUserId;
    }

    public Guid SportsComplexId { get; }
    public Guid BlockedUserId { get; }
}
