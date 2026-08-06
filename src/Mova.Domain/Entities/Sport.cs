using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class Sport
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public SportStatus Status { get; private set; }
    public ICollection<CourtSport> CourtSports { get; private set; } = [];

    private Sport() { }

    public static Sport Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Trim().Length > 255) throw new ArgumentException("Name must not exceed 255 characters.", nameof(name));
        return new Sport { Id = Guid.NewGuid(), Name = name.Trim(), Status = SportStatus.Active };
    }
}

public enum SportStatus
{
    Inactive,
    Active
}
