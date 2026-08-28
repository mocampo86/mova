using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Handlers;

public sealed class UpdateReservationStatusHandler(
    IReservationRepository reservations,
    IAuditLogRepository auditLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUpdateReservationStatusHandler
{
    public async Task<ReservationInfo?> HandleAsync(UpdateReservationStatusCommand command, CancellationToken cancellationToken = default)
    {
        var reservation = await reservations.GetByIdAsync(command.ReservationId, cancellationToken);

        if (reservation is null || reservation.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Reservation not found.");
        }

        try
        {
            switch (command.Status)
            {
                case ReservationStatus.Completed:
                    reservation.MarkCompleted();
                    break;
                case ReservationStatus.NoShow:
                    reservation.MarkNoShow();
                    break;
                default:
                    throw new ConflictException("Only Completed or NoShow statuses are supported.");
            }
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        var auditLog = AuditLog.Create(
            currentUser.UserId,
            command.SportsComplexId,
            "Reservation.UpdateStatus",
            "Reservation",
            reservation.Id.ToString(),
            new { status = command.Status.ToString() });

        await auditLogs.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToInfo(reservation);
    }
}
