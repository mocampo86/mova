namespace Mova.Application.Reservations.Queries;

public sealed class GetRecurringReservationsByComplexQuery
{
    public GetRecurringReservationsByComplexQuery(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? userId = null,
        Guid? courtId = null,
        string? status = null,
        string? sort = null)
    {
        SportsComplexId = sportsComplexId;
        Page = page;
        PageSize = pageSize;
        UserId = userId;
        CourtId = courtId;
        Status = status;
        Sort = sort;
    }

    public Guid SportsComplexId { get; }
    public int Page { get; }
    public int PageSize { get; }
    public Guid? UserId { get; }
    public Guid? CourtId { get; }
    public string? Status { get; }
    public string? Sort { get; }
}
