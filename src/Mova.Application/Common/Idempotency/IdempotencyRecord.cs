namespace Mova.Application.Common.Idempotency;

public sealed class IdempotencyRecord
{
    public required int StatusCode { get; init; }
    public required string ResponseBody { get; init; }
}
