using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Courts.Handlers;

public sealed class UpdateCourtStatusHandler(
    ICourtRepository courts,
    IAuditLogRepository auditLogs,
    IUnitOfWork unitOfWork) : IUpdateCourtStatusHandler
{
    public async Task<CourtInfo> HandleAsync(UpdateCourtStatusCommand command, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(command.CourtId, cancellationToken)
            ?? throw new NotFoundException("Court not found.");

        if (court.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        if (command.Status == CourtStatus.Active)
        {
            court.Activate();
        }
        else
        {
            court.Deactivate();
        }

        var auditLog = AuditLog.Create(
            command.UserId,
            command.SportsComplexId,
            "Court.UpdateStatus",
            "Court",
            court.Id.ToString(),
            new { status = command.Status.ToString() });

        await auditLogs.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourtMapper.ToInfo(court);
    }
}
