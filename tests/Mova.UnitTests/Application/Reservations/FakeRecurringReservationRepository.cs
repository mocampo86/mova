using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Reservations;

public sealed class FakeRecurringReservationRepository : IRecurringReservationRepository
{
    private readonly List<RecurringReservation> _recurringReservations = [];

    public Task<RecurringReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_recurringReservations.FirstOrDefault(r => r.Id == id));
    }

    public Task AddAsync(RecurringReservation recurringReservation, CancellationToken cancellationToken = default)
    {
        _recurringReservations.Add(recurringReservation);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<RecurringReservation> Items, int TotalItems)> GetByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? userId = null,
        Guid? courtId = null,
        RecurringReservationStatus? status = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var query = _recurringReservations
            .Where(r => r.SportsComplexId == sportsComplexId)
            .AsEnumerable();

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

        query = sort?.ToLowerInvariant() switch
        {
            "startdate" => query.OrderBy(r => r.StartDate),
            "startdate:desc" => query.OrderByDescending(r => r.StartDate),
            "enddate" => query.OrderBy(r => r.EndDate),
            "enddate:desc" => query.OrderByDescending(r => r.EndDate),
            "createdat" => query.OrderBy(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalItems = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IReadOnlyList<RecurringReservation> Items, int TotalItems)>((items, totalItems));
    }
}
