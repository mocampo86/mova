using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.CourtRates.Commands;
using Mova.Contracts.CourtRates;
using Mova.Domain.Entities;
using Mova.Domain.Exceptions;

namespace Mova.Application.CourtRates.Handlers;

public sealed class UpdateCourtRateStatusHandler(
    ICourtRepository courts,
    ICourtRateRepository rates,
    IAuditLogRepository auditLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUpdateCourtRateStatusHandler
{
    public async Task<CourtRateInfo> HandleAsync(UpdateCourtRateStatusCommand command, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(command.CourtId, cancellationToken);
        if (court is null || court.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        var rate = await rates.GetByIdAsync(command.RateId, cancellationToken);
        if (rate is null || rate.CourtId != command.CourtId)
        {
            throw new NotFoundException("Court rate not found.");
        }

        if (command.IsActive)
        {
            var existingRates = await rates.GetByCourtIdAsync(command.CourtId, cancellationToken);
            if (existingRates.Any(existing => existing.IsActive && existing.Id != rate.Id && existing.OverlapsWith(rate)))
            {
                throw new RateOverlapException();
            }

            rate.Activate();
        }
        else
        {
            rate.Deactivate();
        }

        var auditLog = AuditLog.Create(
            currentUser.UserId,
            command.SportsComplexId,
            "CourtRate.UpdateStatus",
            "CourtRate",
            rate.Id.ToString(),
            new { courtId = rate.CourtId, isActive = rate.IsActive });

        await auditLogs.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourtRateMapper.ToInfo(rate);
    }
}
