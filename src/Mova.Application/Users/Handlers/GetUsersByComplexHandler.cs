using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public sealed class GetUsersByComplexHandler : IGetUsersByComplexHandler
{
    private readonly ISportsComplexRepository _sportsComplexes;
    private readonly IUserRepository _users;
    private readonly IBlockedUserRepository _blockedUsers;

    public GetUsersByComplexHandler(
        ISportsComplexRepository sportsComplexes,
        IUserRepository users,
        IBlockedUserRepository blockedUsers)
    {
        _sportsComplexes = sportsComplexes;
        _users = users;
        _blockedUsers = blockedUsers;
    }

    public async Task<PagedResult<ComplexUserInfo>> HandleAsync(GetUsersByComplexQuery query, CancellationToken cancellationToken = default)
    {
        var complex = await _sportsComplexes.GetByIdAsync(query.SportsComplexId, cancellationToken);

        if (complex is null)
        {
            throw new NotFoundException("Sports complex not found.");
        }

        var (items, totalItems) = await _users.GetUsersByComplexIdAsync(
            query.SportsComplexId,
            query.Page,
            query.PageSize,
            query.Search,
            query.Sort,
            cancellationToken);

        var userIds = items.Select(u => u.Id).ToList();
        var activeBlocks = await _blockedUsers.GetActiveBlocksByComplexAndUserIdsAsync(
            query.SportsComplexId,
            userIds,
            cancellationToken);

        var blocksByUserId = activeBlocks.ToDictionary(b => b.UserId);
        var mappedItems = items.Select(u => MapUser(u, blocksByUserId.GetValueOrDefault(u.Id))).ToList();

        return PagedResult<ComplexUserInfo>.Create(mappedItems, query.Page, query.PageSize, totalItems);
    }

    private static ComplexUserInfo MapUser(Domain.Entities.User user, Domain.Entities.BlockedUser? block)
    {
        return new ComplexUserInfo
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            PhoneVerified = user.PhoneVerified,
            IsBlocked = block is not null,
            BlockId = block?.Id,
            BlockReason = block?.Reason,
            BlockedUntil = block?.BlockedUntil
        };
    }
}
