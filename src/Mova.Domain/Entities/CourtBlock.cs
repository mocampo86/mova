namespace Mova.Domain.Entities;

public sealed class CourtBlock
{
    public Guid Id { get; private set; }
    public Guid SportsComplexId { get; private set; }
    public Guid CourtId { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public string? Reason { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CourtBlock()
    {
    }

    public static CourtBlock Create(
        Guid sportsComplexId,
        Guid courtId,
        DateTime startAt,
        DateTime endAt,
        Guid createdByUserId,
        string? reason = null)
    {
        if (sportsComplexId == Guid.Empty) throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        if (courtId == Guid.Empty) throw new ArgumentException("CourtId cannot be empty.", nameof(courtId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty.", nameof(createdByUserId));
        if (startAt >= endAt) throw new ArgumentException("StartAt must be earlier than EndAt.", nameof(endAt));

        return new CourtBlock
        {
            Id = Guid.NewGuid(),
            SportsComplexId = sportsComplexId,
            CourtId = courtId,
            StartAt = startAt,
            EndAt = endAt,
            CreatedByUserId = createdByUserId,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }
}
