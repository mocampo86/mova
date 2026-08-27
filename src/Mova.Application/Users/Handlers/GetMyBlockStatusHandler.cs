using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Queries;
using Mova.Contracts.Users;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Users.Handlers;

public sealed class GetMyBlockStatusHandler(
    ISportsComplexRepository sportsComplexes,
    IBlockedUserRepository blockedUsers) : IGetMyBlockStatusHandler
{
    public async Task<MyBlockStatusInfo> HandleAsync(GetMyBlockStatusQuery query, CancellationToken cancellationToken = default)
    {
        var complex = await sportsComplexes.GetByIdAsync(query.ComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        if (complex.Status != ComplexStatus.Active)
        {
            throw new NotFoundException("Sports complex not found.");
        }

        var block = await blockedUsers.GetActiveByComplexAndUserAsync(
            query.ComplexId,
            query.UserId,
            cancellationToken);

        if (block is null)
        {
            return new MyBlockStatusInfo
            {
                IsBlocked = false,
                ComplexId = complex.Id,
                ComplexName = complex.Name
            };
        }

        return MapToInfo(complex, block);
    }

    private static MyBlockStatusInfo MapToInfo(SportsComplex complex, BlockedUser block)
    {
        return new MyBlockStatusInfo
        {
            IsBlocked = true,
            ComplexId = complex.Id,
            ComplexName = complex.Name,
            Reason = block.Reason,
            BlockedAt = block.BlockedAt,
            BlockedUntil = block.BlockedUntil
        };
    }
}
