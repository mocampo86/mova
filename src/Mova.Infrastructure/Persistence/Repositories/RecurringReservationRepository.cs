using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class RecurringReservationRepository(MovaDbContext context) : IRecurringReservationRepository
{
    public Task<RecurringReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.RecurringReservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task AddAsync(RecurringReservation recurringReservation, CancellationToken cancellationToken = default) =>
        context.RecurringReservations.AddAsync(recurringReservation, cancellationToken).AsTask();
}
