using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository(MovaDbContext context) : IReservationRepository
{
    public async Task<IReadOnlyCollection<Reservation>> GetActiveForCourtAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await context.Reservations
            .Where(r => r.CourtId == courtId)
            .Where(r => r.StartAt < end && r.EndAt > start)
            .Where(r => r.Status != ReservationStatus.CancelledByUser && r.Status != ReservationStatus.CancelledByAdmin)
            .ToArrayAsync(cancellationToken);
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
