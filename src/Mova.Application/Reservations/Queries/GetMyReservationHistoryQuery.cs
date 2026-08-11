namespace Mova.Application.Reservations.Queries;

public sealed class GetMyReservationHistoryQuery
{
    public GetMyReservationHistoryQuery(Guid userId, int page, int pageSize)
    {
        UserId = userId;
        Page = page;
        PageSize = pageSize;
    }

    public Guid UserId { get; }

    public int Page { get; }

    public int PageSize { get; }
}
