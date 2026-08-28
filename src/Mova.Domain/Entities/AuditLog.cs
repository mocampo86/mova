using System.Text.Json;

namespace Mova.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? SportsComplexId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? Metadata { get; private set; }

    private AuditLog()
    {
    }

    public static AuditLog Create(
        Guid? userId,
        Guid? sportsComplexId,
        string action,
        string entityType,
        string entityId,
        object? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action cannot be empty.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("EntityType cannot be empty.", nameof(entityType));
        if (string.IsNullOrWhiteSpace(entityId)) throw new ArgumentException("EntityId cannot be empty.", nameof(entityId));

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SportsComplexId = sportsComplexId,
            Action = action.Trim(),
            EntityType = entityType.Trim(),
            EntityId = entityId.Trim(),
            CreatedAt = DateTime.UtcNow,
            Metadata = metadata is null ? null : JsonSerializer.Serialize(metadata)
        };
    }
}
