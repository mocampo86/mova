using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.CourtRates.Commands;
using Mova.Contracts.CourtRates;
using Mova.Domain.Entities;
using Mova.Domain.Exceptions;

namespace Mova.Application.CourtRates.Handlers;

public sealed class CreateCourtRateHandler(
    ICourtRepository courts,
    ICourtRateRepository rates,
    IAuditLogRepository auditLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : ICreateCourtRateHandler
{
    public async Task<CourtRateInfo> HandleAsync(CreateCourtRateCommand command, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(command.CourtId, cancellationToken);
        if (court is null || court.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        var rate = CourtRate.Create(
            command.CourtId,
            CourtRateMapper.ToDaysMask(command.DaysOfWeek),
            command.StartTime,
            command.EndTime,
            command.AmountPerHour);

        var existingRates = await rates.GetByCourtIdAsync(command.CourtId, cancellationToken);
        if (existingRates.Any(existing => existing.IsActive && existing.OverlapsWith(rate)))
        {
            throw new RateOverlapException();
        }

        await rates.AddAsync(rate, cancellationToken);

        var auditLog = AuditLog.Create(
            currentUser.UserId,
            command.SportsComplexId,
            "CourtRate.Create",
            "CourtRate",
            rate.Id.ToString(),
            new { courtId = rate.CourtId, daysOfWeek = rate.DaysOfWeek, startTime = rate.StartTime, endTime = rate.EndTime, amountPerHour = rate.AmountPerHour });

        await auditLogs.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourtRateMapper.ToInfo(rate);
    }
}
