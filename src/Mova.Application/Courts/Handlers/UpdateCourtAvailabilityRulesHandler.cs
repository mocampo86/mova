using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts.Handlers;

public sealed class UpdateCourtAvailabilityRulesHandler(
    ICourtRepository courts,
    ICourtAvailabilityRuleRepository rules,
    IAuditLogRepository auditLogs,
    ICurrentUserContext currentUser,
    IUnitOfWork unitOfWork) : IUpdateCourtAvailabilityRulesHandler
{
    public async Task<IReadOnlyCollection<CourtAvailabilityRuleInfo>> HandleAsync(UpdateCourtAvailabilityRulesCommand command, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(command.CourtId, cancellationToken)
            ?? throw new NotFoundException("Court not found.");
        if (court.SportsComplexId != command.SportsComplexId)
            throw new NotFoundException("Court not found.");

        var existingRules = await rules.GetByCourtIdAsync(command.CourtId, cancellationToken);
        rules.RemoveRange(existingRules);

        var result = new List<CourtAvailabilityRule>();
        foreach (var item in command.Rules)
        {
            var rule = CourtAvailabilityRule.Create(command.CourtId, item.DayOfWeek, item.StartTime, item.EndTime, item.SlotDurationMinutes, item.IsActive);
            await rules.AddAsync(rule, cancellationToken);
            result.Add(rule);
        }

        var auditLog = AuditLog.Create(
            currentUser.UserId,
            command.SportsComplexId,
            "CourtAvailabilityRule.Update",
            "CourtAvailabilityRule",
            court.Id.ToString(),
            new { courtId = court.Id, count = result.Count, days = result.Select(x => x.DayOfWeek.ToString()).ToArray() });

        await auditLogs.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Select(CourtAvailabilityRuleMapper.ToInfo).ToArray();
    }
}
