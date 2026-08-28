using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Complexes.Handlers;

public sealed class UpdateComplexStatusHandler : IUpdateComplexStatusHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;
    private readonly IAuditLogRepository _auditLogs;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComplexStatusHandler(
        ISportsComplexRepository sportsComplexRepository,
        IAuditLogRepository auditLogs,
        IUnitOfWork unitOfWork)
    {
        _sportsComplexRepository = sportsComplexRepository;
        _auditLogs = auditLogs;
        _unitOfWork = unitOfWork;
    }

    public async Task<SportsComplexInfo> HandleAsync(UpdateComplexStatusCommand command, CancellationToken cancellationToken = default)
    {
        var sportsComplex = await _sportsComplexRepository.GetByIdAsync(command.ComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        if (command.Status == ComplexStatus.Active)
        {
            sportsComplex.Activate();
        }
        else
        {
            sportsComplex.Deactivate();
        }

        var auditLog = AuditLog.Create(
            command.UserId,
            sportsComplex.Id,
            "SportsComplex.UpdateStatus",
            "SportsComplex",
            sportsComplex.Id.ToString(),
            new { status = command.Status.ToString() });

        await _auditLogs.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SportsComplexInfoMapper.ToInfo(sportsComplex);
    }
}
