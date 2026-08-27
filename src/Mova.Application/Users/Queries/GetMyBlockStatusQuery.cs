namespace Mova.Application.Users.Queries;

public sealed class GetMyBlockStatusQuery
{
    public GetMyBlockStatusQuery(Guid userId, Guid complexId)
    {
        UserId = userId;
        ComplexId = complexId;
    }

    public Guid UserId { get; }

    public Guid ComplexId { get; }
}
