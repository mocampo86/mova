namespace Mova.Application.Users.Queries;

public sealed class SearchUsersQuery
{
    public SearchUsersQuery(
        Guid sportsComplexId,
        string? search,
        int page,
        int pageSize,
        string? sort = null)
    {
        SportsComplexId = sportsComplexId;
        Search = search;
        Page = page;
        PageSize = pageSize;
        Sort = sort;
    }

    public Guid SportsComplexId { get; }
    public string? Search { get; }
    public int Page { get; }
    public int PageSize { get; }
    public string? Sort { get; }
}
