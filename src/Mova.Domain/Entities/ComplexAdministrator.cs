using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class ComplexAdministrator
{
    public Guid Id { get; private set; }
    public Guid SportsComplexId { get; private set; }
    public Guid UserId { get; private set; }
    public Role Role { get; private set; }
    public AdministratorStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ComplexAdministrator()
    {
    }

    public static ComplexAdministrator Create(Guid sportsComplexId, Guid userId, Role role)
    {
        if (sportsComplexId == Guid.Empty)
        {
            throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        }

        if (role != Role.ComplexAdmin && role != Role.SuperAdmin)
        {
            throw new ArgumentException("Complex administrator role must be ComplexAdmin or SuperAdmin.", nameof(role));
        }

        return new ComplexAdministrator
        {
            Id = Guid.NewGuid(),
            SportsComplexId = sportsComplexId,
            UserId = userId,
            Role = role,
            Status = AdministratorStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        if (Status == AdministratorStatus.Inactive)
        {
            return;
        }

        Status = AdministratorStatus.Inactive;
    }

    public void Activate()
    {
        if (Status == AdministratorStatus.Active)
        {
            return;
        }

        Status = AdministratorStatus.Active;
    }
}
