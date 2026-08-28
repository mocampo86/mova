using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;
using Mova.Domain.Entities;

namespace Mova.Application.Complexes.Handlers;

public sealed class UpdateRecurringReservationSettingsHandler(
    ISportsComplexRepository sportsComplexRepository,
    IAuditLogRepository auditLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUpdateRecurringReservationSettingsHandler
{
    public async Task<SportsComplexInfo> HandleAsync(UpdateRecurringReservationSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var complex = await sportsComplexRepository.GetByIdAsync(command.ComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        complex.UpdateRecurringReservationSettings(command.AllowUserRecurringReservations);

        var auditLog = AuditLog.Create(
            currentUser.UserId,
            complex.Id,
            "SportsComplex.UpdateRecurringReservationSettings",
            "SportsComplex",
            complex.Id.ToString(),
            new { allowUserRecurringReservations = command.AllowUserRecurringReservations });

        await auditLogs.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SportsComplexInfoMapper.ToInfo(complex);
    }
}
