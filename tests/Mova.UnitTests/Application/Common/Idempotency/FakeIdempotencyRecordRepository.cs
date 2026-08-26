using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Common.Idempotency;

public sealed class FakeIdempotencyRecordRepository : IIdempotencyRecordRepository
{
    private readonly List<IdempotencyRecord> _records = [];

    public Task<IdempotencyRecord?> GetAsync(
        string actorKey,
        string scope,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var record = _records.FirstOrDefault(r =>
            r.ActorKey == actorKey &&
            r.Scope == scope &&
            r.IdempotencyKey == idempotencyKey &&
            r.ExpiresAt > DateTime.UtcNow);

        return Task.FromResult(record);
    }

    public Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        _records.Add(record);
        return Task.CompletedTask;
    }

    public Task<int> RemoveExpiredAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        var count = _records.RemoveAll(r => r.ExpiresAt < before);
        return Task.FromResult(count);
    }
}
