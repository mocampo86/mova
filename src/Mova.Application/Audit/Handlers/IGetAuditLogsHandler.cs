using Mova.Application.Audit.Queries;
using Mova.Contracts.Audit;
using Mova.Contracts.Common;

namespace Mova.Application.Audit.Handlers;

public interface IGetAuditLogsHandler
{
    Task<PagedResult<AuditLogInfo>> HandleAsync(GetAuditLogsQuery query, int page, int pageSize, CancellationToken cancellationToken = default);
}
