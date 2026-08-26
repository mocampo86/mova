namespace Mova.Domain.Entities;

public sealed class IdempotencyRecord
{
    public Guid Id { get; private set; }
    public string ActorKey { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }
    public string ResponseBody { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private IdempotencyRecord()
    {
    }

    public static IdempotencyRecord Create(
        string actorKey,
        string scope,
        string idempotencyKey,
        int statusCode,
        string responseBody,
        DateTime? now = null,
        TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(actorKey)) throw new ArgumentException("ActorKey cannot be empty.", nameof(actorKey));
        if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentException("Scope cannot be empty.", nameof(scope));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("IdempotencyKey cannot be empty.", nameof(idempotencyKey));

        var createdAt = now ?? DateTime.UtcNow;
        return new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            ActorKey = actorKey,
            Scope = scope,
            IdempotencyKey = idempotencyKey,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            CreatedAt = createdAt,
            ExpiresAt = createdAt + (ttl ?? TimeSpan.FromHours(24))
        };
    }
}
