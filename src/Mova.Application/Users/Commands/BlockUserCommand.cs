namespace Mova.Application.Users.Commands;

public sealed class BlockUserCommand
{
    public BlockUserCommand(
        Guid sportsComplexId,
        Guid userId,
        Guid blockedByUserId,
        string? reason = null,
        DateTime? blockedUntil = null)
    {
        SportsComplexId = sportsComplexId;
        UserId = userId;
        BlockedByUserId = blockedByUserId;
        Reason = reason;
        BlockedUntil = blockedUntil;
    }

    public Guid SportsComplexId { get; }
    public Guid UserId { get; }
    public Guid BlockedByUserId { get; }
    public string? Reason { get; }
    public DateTime? BlockedUntil { get; }
}
