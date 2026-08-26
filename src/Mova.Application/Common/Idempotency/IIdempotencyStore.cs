namespace Mova.Application.Common.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(
        string actorKey,
        string scope,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task StoreAsync(
        string actorKey,
        string scope,
        string idempotencyKey,
        int statusCode,
        string responseBody,
        CancellationToken cancellationToken = default);
}
