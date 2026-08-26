using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IIdempotencyRecordRepository
{
    Task<IdempotencyRecord?> GetAsync(
        string actorKey,
        string scope,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);

    Task<int> RemoveExpiredAsync(DateTime before, CancellationToken cancellationToken = default);
}
