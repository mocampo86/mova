using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository(MovaDbContext context) : IReservationRepository
{
    private const int MaxPageSize = 100;

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .Include(r => r.CancelledByUser)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default) =>
        context.Reservations.AddAsync(reservation, cancellationToken).AsTask();

    public Task AddRangeAsync(IEnumerable<Reservation> reservations, CancellationToken cancellationToken = default) =>
        context.Reservations.AddRangeAsync(reservations, cancellationToken);

    public async Task<IReadOnlyCollection<Reservation>> GetActiveForCourtAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await context.Reservations
            .Where(r => r.CourtId == courtId)
            .Where(r => r.StartAt < end && r.EndAt > start)
            .Where(r => r.Status != ReservationStatus.CancelledByUser && r.Status != ReservationStatus.CancelledByAdmin)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Reservation> Items, int TotalItems)> GetByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? courtId = null,
        ReservationStatus? status = null,
        DateTime? date = null,
        string? sort = null,
        Guid? userId = null,
        int utcOffsetMinutes = 0,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        IQueryable<Reservation> query = context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .Include(r => r.CancelledByUser)
            .Where(r => r.SportsComplexId == sportsComplexId);

        if (courtId.HasValue)
        {
            query = query.Where(r => r.CourtId == courtId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (date.HasValue)
        {
            var dayStart = date.Value.Date.AddMinutes(utcOffsetMinutes);
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(r => r.StartAt >= dayStart && r.StartAt < dayEnd);
        }

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        var sortBy = sort?.Split(':', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var sortField = sortBy.Length > 0 ? sortBy[0] : "startAt";
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
            "endat" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.EndAt)
                : query.OrderByDescending(r => r.EndAt),
            _ => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.StartAt)
                : query.OrderByDescending(r => r.StartAt)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<(IReadOnlyList<Reservation> Items, int TotalItems)> GetUpcomingByUserIdAsync(
        Guid userId,
        DateTime from,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        var query = context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .Include(r => r.CancelledByUser)
            .Where(r => r.UserId == userId)
            .Where(r => r.StartAt >= from)
            .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
            .OrderBy(r => r.StartAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<(IReadOnlyList<Reservation> Items, int TotalItems)> GetHistoryByUserIdAsync(
        Guid userId,
        DateTime from,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        var query = context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .Include(r => r.CancelledByUser)
            .Where(r => r.UserId == userId)
            .Where(r => r.StartAt < from || r.Status == ReservationStatus.Completed || r.Status == ReservationStatus.CancelledByUser || r.Status == ReservationStatus.CancelledByAdmin || r.Status == ReservationStatus.NoShow)
            .OrderByDescending(r => r.StartAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<bool> HasOverlappingActiveReservationAsync(
        Guid courtId,
        DateTime start,
        DateTime end,
        Guid? excludeReservationId = null,
        Guid? excludeRecurringReservationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Reservations
            .Where(r => r.CourtId == courtId)
            .Where(r => r.StartAt < end && r.EndAt > start)
            .Where(r => r.Status != ReservationStatus.CancelledByUser && r.Status != ReservationStatus.CancelledByAdmin);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        if (excludeRecurringReservationId.HasValue)
        {
            query = query.Where(r => r.RecurringReservationId != excludeRecurringReservationId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetFutureActiveByRecurringReservationIdAsync(
        Guid recurringReservationId,
        DateTime from,
        CancellationToken cancellationToken = default)
    {
        return await context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .Where(r => r.RecurringReservationId == recurringReservationId)
            .Where(r => r.StartAt >= from)
            .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
            .OrderBy(r => r.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int Confirmed, int Cancelled, int Completed)> GetTodayStatusCountsByComplexIdAsync(
        Guid sportsComplexId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var query = context.Reservations
            .Where(r => r.SportsComplexId == sportsComplexId && r.StartAt >= start && r.StartAt < end);

        var confirmed = await query.CountAsync(r => r.Status == ReservationStatus.Confirmed, cancellationToken);
        var cancelled = await query.CountAsync(r => r.Status == ReservationStatus.CancelledByUser || r.Status == ReservationStatus.CancelledByAdmin, cancellationToken);
        var completed = await query.CountAsync(r => r.Status == ReservationStatus.Completed, cancellationToken);

        return (confirmed, cancelled, completed);
    }
}
