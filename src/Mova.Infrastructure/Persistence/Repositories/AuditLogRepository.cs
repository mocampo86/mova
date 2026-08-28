using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Audit.Queries;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(MovaDbContext context) : IAuditLogRepository
{
    private const int MaxPageSize = 100;

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default) =>
        context.AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalItems)> SearchAsync(
        GetAuditLogsQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        IQueryable<AuditLog> dbQuery = context.AuditLogs
            .OrderByDescending(x => x.CreatedAt);

        if (query.SportsComplexId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.SportsComplexId == query.SportsComplexId.Value);
        }

        if (query.UserId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            dbQuery = dbQuery.Where(x => x.Action == query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            dbQuery = dbQuery.Where(x => x.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            dbQuery = dbQuery.Where(x => x.EntityId == query.EntityId);
        }

        if (query.From.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAt <= query.To.Value);
        }

        var totalItems = await dbQuery.CountAsync(cancellationToken);
        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }
}
