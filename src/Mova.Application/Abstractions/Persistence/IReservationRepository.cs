using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IReservationRepository
{
    Task<IReadOnlyCollection<Reservation>> GetActiveForCourtAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
