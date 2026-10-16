namespace Mova.Domain.Entities;

public sealed class UserFavoriteComplex
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid SportsComplexId { get; private set; }
    public SportsComplex SportsComplex { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private UserFavoriteComplex()
    {
    }

    public static UserFavoriteComplex Create(Guid userId, Guid sportsComplexId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        }

        if (sportsComplexId == Guid.Empty)
        {
            throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        }

        return new UserFavoriteComplex
        {
            UserId = userId,
            SportsComplexId = sportsComplexId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
