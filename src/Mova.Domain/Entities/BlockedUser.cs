using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class BlockedUser
{
    public Guid Id { get; private set; }

    public Guid SportsComplexId { get; private set; }

    public Guid UserId { get; private set; }

    public string? Reason { get; private set; }

    public DateTime BlockedAt { get; private set; }

    public DateTime? BlockedUntil { get; private set; }

    public Guid BlockedByUserId { get; private set; }

    public BlockedUserStatus Status { get; private set; }

    private BlockedUser()
    {
    }

    public static BlockedUser Create(
        Guid sportsComplexId,
        Guid userId,
        Guid blockedByUserId,
        string? reason = null,
        DateTime? blockedUntil = null)
    {
        if (sportsComplexId == Guid.Empty)
        {
            throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        }

        if (blockedByUserId == Guid.Empty)
        {
            throw new ArgumentException("BlockedByUserId cannot be empty.", nameof(blockedByUserId));
        }

        return new BlockedUser
        {
            Id = Guid.NewGuid(),
            SportsComplexId = sportsComplexId,
            UserId = userId,
            BlockedByUserId = blockedByUserId,
            Reason = reason,
            BlockedAt = DateTime.UtcNow,
            BlockedUntil = blockedUntil,
            Status = BlockedUserStatus.Active
        };
    }

    public void Lift()
    {
        if (Status == BlockedUserStatus.Lifted)
        {
            return;
        }

        Status = BlockedUserStatus.Lifted;
    }
}
