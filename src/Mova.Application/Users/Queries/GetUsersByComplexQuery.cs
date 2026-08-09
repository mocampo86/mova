namespace Mova.Application.Users.Queries;

public sealed class GetUsersByComplexQuery
{
    public GetUsersByComplexQuery(
        Guid sportsComplexId,
        int page,
        int pageSize,
        string? search = null,
        string? sort = null)
    {
        SportsComplexId = sportsComplexId;
        Page = page;
        PageSize = pageSize;
        Search = search;
        Sort = sort;
    }

    public Guid SportsComplexId { get; }
    public int Page { get; }
    public int PageSize { get; }
    public string? Search { get; }
    public string? Sort { get; }
}
