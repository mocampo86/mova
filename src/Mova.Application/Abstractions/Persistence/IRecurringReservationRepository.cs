using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Abstractions.Persistence;

public interface IRecurringReservationRepository
{
    Task<RecurringReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(RecurringReservation recurringReservation, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<RecurringReservation> Items, int TotalItems)> GetByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? userId = null,
        Guid? courtId = null,
        RecurringReservationStatus? status = null,
        string? sort = null,
        CancellationToken cancellationToken = default);
}
