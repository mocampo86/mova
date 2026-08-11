namespace Mova.Application.Users.Queries;

public sealed class GetUserDashboardQuery
{
    public GetUserDashboardQuery(Guid userId, int upcomingPage, int upcomingPageSize, int historyPageSize)
    {
        UserId = userId;
        UpcomingPage = upcomingPage;
        UpcomingPageSize = upcomingPageSize;
        HistoryPageSize = historyPageSize;
    }

    public Guid UserId { get; }

    public int UpcomingPage { get; }

    public int UpcomingPageSize { get; }

    public int HistoryPageSize { get; }
}
