namespace Mova.Domain.Entities;

public sealed class CancellationPolicy
{
    public Guid Id { get; private set; }

    public Guid SportsComplexId { get; private set; }

    public SportsComplex? SportsComplex { get; private set; }

    public int MinimumHours { get; private set; }

    public bool AllowUserCancellation { get; private set; }

    private CancellationPolicy()
    {
    }

    public static CancellationPolicy Create(Guid sportsComplexId, int minimumHours, bool allowUserCancellation)
    {
        if (sportsComplexId == Guid.Empty)
        {
            throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        }

        if (minimumHours < 0)
        {
            throw new ArgumentException("MinimumHours must be greater than or equal to zero.", nameof(minimumHours));
        }

        return new CancellationPolicy
        {
            Id = Guid.NewGuid(),
            SportsComplexId = sportsComplexId,
            MinimumHours = minimumHours,
            AllowUserCancellation = allowUserCancellation
        };
    }

    public void Update(int minimumHours, bool allowUserCancellation)
    {
        if (minimumHours < 0)
        {
            throw new ArgumentException("MinimumHours must be greater than or equal to zero.", nameof(minimumHours));
        }

        MinimumHours = minimumHours;
        AllowUserCancellation = allowUserCancellation;
    }
}
