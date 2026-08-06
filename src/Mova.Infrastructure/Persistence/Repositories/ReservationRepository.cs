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
}
