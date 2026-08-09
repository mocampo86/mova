using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Abstractions.Persistence;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Reservation>> GetActiveForCourtAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Reservation> Items, int TotalItems)> GetByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        Guid? courtId = null,
        ReservationStatus? status = null,
        DateTime? date = null,
        string? sort = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingActiveReservationAsync(
        Guid courtId,
        DateTime start,
        DateTime end,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default);
    Task<(int Confirmed, int Cancelled, int Completed)> GetTodayStatusCountsByComplexIdAsync(Guid sportsComplexId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
