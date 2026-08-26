using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class IdempotencyRecordRepository(MovaDbContext context) : IIdempotencyRecordRepository
{
    public Task<IdempotencyRecord?> GetAsync(
        string actorKey,
        string scope,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return context.IdempotencyRecords
            .AsNoTracking()
            .Where(r => r.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(
                r => r.ActorKey == actorKey && r.Scope == scope && r.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    public Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        return context.IdempotencyRecords.AddAsync(record, cancellationToken).AsTask();
    }

    public async Task<int> RemoveExpiredAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        return await context.IdempotencyRecords
            .Where(r => r.ExpiresAt < before)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
