using Mova.Application.Abstractions.Persistence;

namespace Mova.Application.Common.Idempotency;

public sealed class IdempotencyStore(IIdempotencyRecordRepository repository) : IIdempotencyStore
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    public async Task<IdempotencyRecord?> GetAsync(
        string actorKey,
        string scope,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var record = await repository.GetAsync(actorKey, scope, idempotencyKey, cancellationToken);
        if (record is null)
        {
            return null;
        }

        return new IdempotencyRecord
        {
            StatusCode = record.StatusCode,
            ResponseBody = record.ResponseBody
        };
    }

    public async Task StoreAsync(
        string actorKey,
        string scope,
        string idempotencyKey,
        int statusCode,
        string responseBody,
        CancellationToken cancellationToken = default)
    {
        var record = Mova.Domain.Entities.IdempotencyRecord.Create(
            actorKey,
            scope,
            idempotencyKey,
            statusCode,
            responseBody,
            DateTime.UtcNow,
            DefaultTtl);

        await repository.AddAsync(record, cancellationToken);
    }
}
