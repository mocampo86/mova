namespace Mova.Domain.Entities;

public sealed class CourtSport
{
    public Guid CourtId { get; private set; }
    public Court Court { get; private set; } = null!;
    public Guid SportId { get; private set; }
    public Sport Sport { get; private set; } = null!;

    private CourtSport() { }

    internal CourtSport(Guid courtId, Guid sportId)
    {
        CourtId = courtId;
        SportId = sportId;
    }
}
