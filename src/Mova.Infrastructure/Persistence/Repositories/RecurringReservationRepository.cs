using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class RecurringReservationRepository(MovaDbContext context) : IRecurringReservationRepository
{
    private const int MaxPageSize = 100;

    public Task<RecurringReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.RecurringReservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task AddAsync(RecurringReservation recurringReservation, CancellationToken cancellationToken = default) =>
        context.RecurringReservations.AddAsync(recurringReservation, cancellationToken).AsTask();

    public async Task<(IReadOnlyList<RecurringReservation> Items, int TotalItems)> GetByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? userId = null,
        Guid? courtId = null,
        RecurringReservationStatus? status = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        IQueryable<RecurringReservation> query = context.RecurringReservations
            .AsNoTracking()
            .Include(r => r.Court)
            .Include(r => r.User)
            .Where(r => r.SportsComplexId == sportsComplexId);

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (courtId.HasValue)
        {
            query = query.Where(r => r.CourtId == courtId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var sortBy = sort?.Split(':', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var sortField = sortBy.Length > 0 ? sortBy[0] : "createdAt";
        var sortDirection = sortBy.Length > 1 ? sortBy[1] : "desc";

        query = sortField.ToLowerInvariant() switch
        {
            "court" or "courtname" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.Court!.Name)
                : query.OrderByDescending(r => r.Court!.Name),
            "user" or "username" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.User!.FullName)
                : query.OrderByDescending(r => r.User!.FullName),
            "status" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.Status)
                : query.OrderByDescending(r => r.Status),
            "startdate" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.StartDate)
                : query.OrderByDescending(r => r.StartDate),
            "enddate" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.EndDate)
                : query.OrderByDescending(r => r.EndDate),
            _ => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.CreatedAt)
                : query.OrderByDescending(r => r.CreatedAt)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }
}
