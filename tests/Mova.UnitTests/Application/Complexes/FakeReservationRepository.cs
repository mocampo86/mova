using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Complexes;

public sealed class FakeReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = [];

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

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _reservations.Add(reservation);
        return Task.CompletedTask;
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
