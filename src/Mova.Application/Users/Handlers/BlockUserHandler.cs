using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Contracts.Users;
using Mova.Domain.Entities;

namespace Mova.Application.Users.Handlers;

public sealed class BlockUserHandler : IBlockUserHandler
{
    private readonly ISportsComplexRepository _sportsComplexes;
    private readonly IUserRepository _users;
    private readonly IBlockedUserRepository _blockedUsers;
    private readonly IUnitOfWork _unitOfWork;

    public BlockUserHandler(
        ISportsComplexRepository sportsComplexes,
        IUserRepository users,
        IBlockedUserRepository blockedUsers,
        IUnitOfWork unitOfWork)
    {
        _sportsComplexes = sportsComplexes;
        _users = users;
        _blockedUsers = blockedUsers;
        _unitOfWork = unitOfWork;
    }

    public async Task<BlockedUserInfo> HandleAsync(BlockUserCommand command, CancellationToken cancellationToken = default)
    {
        var complex = await _sportsComplexes.GetByIdAsync(command.SportsComplexId, cancellationToken);

        if (complex is null)
        {
            throw new NotFoundException("Sports complex not found.");
        }

        var user = await _users.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var existingBlock = await _blockedUsers.GetActiveByComplexAndUserAsync(command.SportsComplexId, command.UserId, cancellationToken);

        if (existingBlock is not null)
        {
            throw new ConflictException("User is already blocked in this complex.");
        }

        var block = BlockedUser.Create(
            command.SportsComplexId,
            command.UserId,
            command.BlockedByUserId,
            command.Reason,
            command.BlockedUntil);

        await _blockedUsers.AddAsync(block, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToInfo(block);
    }

    private static BlockedUserInfo MapToInfo(BlockedUser block)
    {
        return new BlockedUserInfo
        {
            Id = block.Id,
            SportsComplexId = block.SportsComplexId,
            UserId = block.UserId,
            Reason = block.Reason,
            BlockedAt = block.BlockedAt,
            BlockedUntil = block.BlockedUntil,
            BlockedByUserId = block.BlockedByUserId,
            Status = block.Status.ToString()
        };
    }
}
