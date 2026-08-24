namespace Mova.Application.Reservations.Queries;

public sealed class GetReservationsByComplexQuery
{
    public GetReservationsByComplexQuery(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? courtId = null,
        string? status = null,
        DateOnly? date = null,
        string? sort = null,
        Guid? userId = null)
    {
        SportsComplexId = sportsComplexId;
        Page = page;
        PageSize = pageSize;
        CourtId = courtId;
        Status = status;
        Date = date;
        Sort = sort;
        UserId = userId;
    }

    public Guid SportsComplexId { get; }
    public int Page { get; }
    public int PageSize { get; }
    public Guid? CourtId { get; }
    public string? Status { get; }
    public DateOnly? Date { get; }
    public string? Sort { get; }
    public Guid? UserId { get; }
}
