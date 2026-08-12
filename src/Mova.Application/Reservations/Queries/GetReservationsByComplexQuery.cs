namespace Mova.Application.Reservations.Queries;

public sealed class GetReservationsByComplexQuery
{
    public GetReservationsByComplexQuery(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? courtId = null,
        string? status = null,
        DateTime? date = null,
        string? sort = null,
        Guid? userId = null,
        int utcOffsetMinutes = 0)
    {
        SportsComplexId = sportsComplexId;
        Page = page;
        PageSize = pageSize;
        CourtId = courtId;
        Status = status;
        Date = date;
        Sort = sort;
        UserId = userId;
        UtcOffsetMinutes = utcOffsetMinutes;
    }

    public Guid SportsComplexId { get; }
    public int Page { get; }
    public int PageSize { get; }
    public Guid? CourtId { get; }
    public string? Status { get; }
    public DateTime? Date { get; }
    public string? Sort { get; }
    public Guid? UserId { get; }
    public int UtcOffsetMinutes { get; }
}
