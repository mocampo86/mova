using Mova.Application.Abstractions.Persistence;
using Mova.Application.Audit.Queries;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Audit;

public sealed class FakeAuditLogRepository : IAuditLogRepository
{
    public List<AuditLog> AuditLogs { get; } = [];

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        AuditLogs.Add(auditLog);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<AuditLog> Items, int TotalItems)> SearchAsync(
        GetAuditLogsQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filtered = AuditLogs
            .Where(x =>
                (!query.SportsComplexId.HasValue || x.SportsComplexId == query.SportsComplexId.Value) &&
                (!query.UserId.HasValue || x.UserId == query.UserId.Value) &&
                (string.IsNullOrWhiteSpace(query.Action) || x.Action == query.Action) &&
                (string.IsNullOrWhiteSpace(query.EntityType) || x.EntityType == query.EntityType) &&
                (string.IsNullOrWhiteSpace(query.EntityId) || x.EntityId == query.EntityId) &&
                (!query.From.HasValue || x.CreatedAt >= query.From.Value) &&
                (!query.To.HasValue || x.CreatedAt <= query.To.Value))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IReadOnlyList<AuditLog> Items, int TotalItems)>((items, filtered.Count));
    }
}
