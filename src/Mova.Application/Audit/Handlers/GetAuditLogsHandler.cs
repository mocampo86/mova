using Mova.Application.Abstractions.Persistence;
using Mova.Application.Audit.Queries;
using Mova.Contracts.Audit;
using Mova.Contracts.Common;

namespace Mova.Application.Audit.Handlers;

public sealed class GetAuditLogsHandler(IAuditLogRepository auditLogRepository) : IGetAuditLogsHandler
{
    public async Task<PagedResult<AuditLogInfo>> HandleAsync(
        GetAuditLogsQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await auditLogRepository.SearchAsync(query, page, pageSize, cancellationToken);

        return PagedResult<AuditLogInfo>.Create(
            items.Select(x => new AuditLogInfo
            {
                Id = x.Id,
                UserId = x.UserId,
                SportsComplexId = x.SportsComplexId,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                CreatedAt = x.CreatedAt,
                Metadata = x.Metadata
            }).ToList(),
            page,
            pageSize,
            totalItems);
    }
}
