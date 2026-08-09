using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Contracts.Users;
using Mova.Domain.Entities;

namespace Mova.Application.Users.Handlers;

public sealed class UnblockUserHandler : IUnblockUserHandler
{
    private readonly IBlockedUserRepository _blockedUsers;
    private readonly IUnitOfWork _unitOfWork;

    public UnblockUserHandler(IBlockedUserRepository blockedUsers, IUnitOfWork unitOfWork)
    {
        _blockedUsers = blockedUsers;
        _unitOfWork = unitOfWork;
    }

    public async Task<BlockedUserInfo> HandleAsync(UnblockUserCommand command, CancellationToken cancellationToken = default)
    {
        var block = await _blockedUsers.GetByIdAsync(command.BlockedUserId, cancellationToken);

        if (block is null || block.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Block record not found.");
        }

        block.Lift();
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
