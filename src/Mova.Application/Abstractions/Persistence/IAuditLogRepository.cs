using Mova.Application.Audit.Queries;
using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AuditLog> Items, int TotalItems)> SearchAsync(
        GetAuditLogsQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
