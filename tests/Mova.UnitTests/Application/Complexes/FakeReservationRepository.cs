using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = [];

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_reservations.FirstOrDefault(r => r.Id == id));
    }

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _reservations.Add(reservation);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Reservation>> GetActiveForCourtAsync(
        Guid courtId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var result = _reservations
            .Where(r => r.CourtId == courtId)
            .Where(r => r.StartAt < end && r.EndAt > start)
            .Where(r => r.Status != ReservationStatus.CancelledByUser && r.Status != ReservationStatus.CancelledByAdmin)
            .ToList();

        return Task.FromResult(result as IReadOnlyCollection<Reservation>);
    }

    public Task<(IReadOnlyList<Reservation> Items, int TotalItems)> GetByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? courtId = null,
        ReservationStatus? status = null,
        DateTime? date = null,
        string? sort = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _reservations.Where(r => r.SportsComplexId == sportsComplexId);

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
            var dayStart = date.Value.Date;
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
                ? query.OrderBy(r => r.Court?.Name)
                : query.OrderByDescending(r => r.Court?.Name),
            "user" or "username" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(r => r.User?.FullName)
                : query.OrderByDescending(r => r.User?.FullName),
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

        var list = query.ToList();
        var totalItems = list.Count;

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;

        var items = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IReadOnlyList<Reservation> Items, int TotalItems)>((items, totalItems));
    }

    public Task<bool> HasOverlappingActiveReservationAsync(
        Guid courtId,
        DateTime start,
        DateTime end,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _reservations
            .Where(r => r.CourtId == courtId)
            .Where(r => r.StartAt < end && r.EndAt > start)
            .Where(r => r.Status != ReservationStatus.CancelledByUser && r.Status != ReservationStatus.CancelledByAdmin);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return Task.FromResult(query.Any());
    }

    public Task<(int Confirmed, int Cancelled, int Completed)> GetTodayStatusCountsByComplexIdAsync(
        Guid sportsComplexId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var query = _reservations.Where(r => r.SportsComplexId == sportsComplexId && r.StartAt >= start && r.StartAt < end);

        var confirmed = query.Count(r => r.Status == ReservationStatus.Confirmed);
        var cancelled = query.Count(r => r.Status == ReservationStatus.CancelledByUser || r.Status == ReservationStatus.CancelledByAdmin);
        var completed = query.Count(r => r.Status == ReservationStatus.Completed);

        return Task.FromResult((confirmed, cancelled, completed));
    }
}
