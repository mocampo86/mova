namespace Mova.Application.Reservations.Queries;

public sealed class GetMyUpcomingReservationsQuery
{
    public GetMyUpcomingReservationsQuery(Guid userId, DateTime from, int page, int pageSize)
    {
        UserId = userId;
        From = from;
        Page = page;
        PageSize = pageSize;
    }

    public Guid UserId { get; }
    public DateTime From { get; }
    public int Page { get; }
    public int PageSize { get; }
}
