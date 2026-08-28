using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Contracts.Users;
using Mova.Domain.Entities;

namespace Mova.Application.Users.Handlers;

public sealed class UnblockUserHandler : IUnblockUserHandler
{
    private readonly IBlockedUserRepository _blockedUsers;
    private readonly IAuditLogRepository _auditLogs;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UnblockUserHandler(
        IBlockedUserRepository blockedUsers,
        IAuditLogRepository auditLogs,
        ICurrentUserContext currentUser,
        IUnitOfWork unitOfWork)
    {
        _blockedUsers = blockedUsers;
        _auditLogs = auditLogs;
        _currentUser = currentUser;
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

        var auditLog = AuditLog.Create(
            _currentUser.UserId,
            command.SportsComplexId,
            "BlockedUser.Unblock",
            "BlockedUser",
            block.Id.ToString(),
            new { userId = block.UserId, sportsComplexId = block.SportsComplexId });

        await _auditLogs.AddAsync(auditLog, cancellationToken);
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
