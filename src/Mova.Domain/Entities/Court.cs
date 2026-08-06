using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class Court
{
    public Guid Id { get; private set; }
    public Guid SportsComplexId { get; private set; }
    public SportsComplex? SportsComplex { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string SurfaceType { get; private set; } = null!;
    public bool Indoor { get; private set; }
    public CourtStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public ICollection<CourtSport> CourtSports { get; private set; } = [];

    private Court() { }

    public static Court Create(Guid sportsComplexId, string name, string description, string surfaceType, bool indoor, IEnumerable<Guid>? sportIds = null)
    {
        if (sportsComplexId == Guid.Empty) throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        Validate(name, description, surfaceType);
        var court = new Court
        {
            Id = Guid.NewGuid(), SportsComplexId = sportsComplexId, Name = name.Trim(),
            Description = description.Trim(), SurfaceType = surfaceType.Trim(), Indoor = indoor,
            Status = CourtStatus.Active, CreatedAt = DateTime.UtcNow
        };
        foreach (var sportId in sportIds ?? [])
        {
            if (sportId == Guid.Empty) throw new ArgumentException("Sport identifiers cannot be empty.", nameof(sportIds));
            court.CourtSports.Add(new CourtSport(court.Id, sportId));
        }
        return court;
    }

    public bool CanAcceptReservations() => Status == CourtStatus.Active && CourtSports.Any(x => x.Sport?.Status == SportStatus.Active);

    private static void Validate(string name, string description, string surfaceType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceType);
        if (name.Trim().Length > 255) throw new ArgumentException("Name must not exceed 255 characters.", nameof(name));
        if (description.Trim().Length > 2000) throw new ArgumentException("Description must not exceed 2000 characters.", nameof(description));
        if (surfaceType.Trim().Length > 100) throw new ArgumentException("Surface type must not exceed 100 characters.", nameof(surfaceType));
    }
}
