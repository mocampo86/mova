using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IRecurringReservationRepository
{
    Task<RecurringReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(RecurringReservation recurringReservation, CancellationToken cancellationToken = default);
}
