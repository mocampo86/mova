using Mova.Application.Abstractions.Persistence;
using Mova.Application.Reservations;
using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Reservations;
using Mova.Contracts.Users;
using Mova.Domain.Enums;

namespace Mova.Application.Users.Handlers;

public sealed class GetUserDashboardHandler(
    IUserRepository users,
    IReservationRepository reservations,
    IBlockedUserRepository blockedUsers,
    ISportsComplexRepository sportsComplexes) : IGetUserDashboardHandler
{
    public async Task<UserDashboardInfo> HandleAsync(GetUserDashboardQuery query, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(query.UserId, cancellationToken)
            ?? throw new Common.Exceptions.NotFoundException("User not found.");

        var now = DateTime.UtcNow;

        var (upcomingItems, upcomingTotal) = await reservations.GetUpcomingByUserIdAsync(
            query.UserId,
            now,
            query.UpcomingPage,
            query.UpcomingPageSize,
            cancellationToken);

        var (historyItems, historyTotal) = await reservations.GetHistoryByUserIdAsync(
            query.UserId,
            now,
            1,
            query.HistoryPageSize,
            cancellationToken);

        var activeBlocks = await blockedUsers.GetActiveBlocksByUserIdAsync(query.UserId, cancellationToken);
        var complexIds = activeBlocks.Select(b => b.SportsComplexId).Distinct().ToList();
        var complexNames = new Dictionary<Guid, string>();

        foreach (var complexId in complexIds)
        {
            var complex = await sportsComplexes.GetByIdAsync(complexId, cancellationToken);
            complexNames[complexId] = complex?.Name ?? string.Empty;
        }

        return new UserDashboardInfo
        {
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                PhoneVerified = user.PhoneVerified
            },
            UpcomingReservations = PagedResult<ReservationInfo>.Create(
                upcomingItems.Select(ReservationMapper.ToInfo).ToList(),
                upcomingItems.Count == 0 ? 1 : query.UpcomingPage,
                query.UpcomingPageSize,
                upcomingTotal),
            HistorySummary = new ReservationHistorySummaryInfo
            {
                TotalItems = historyTotal,
                RecentReservations = historyItems.Select(ReservationMapper.ToInfo).ToList()
            },
            ActiveBlocks = activeBlocks.Select(b => new UserBlockInfo
            {
                Id = b.Id,
                ComplexId = b.SportsComplexId,
                ComplexName = complexNames.GetValueOrDefault(b.SportsComplexId, string.Empty),
                Reason = b.Reason,
                BlockedAt = b.BlockedAt,
                BlockedUntil = b.BlockedUntil
            }).ToList()
        };
    }
}
