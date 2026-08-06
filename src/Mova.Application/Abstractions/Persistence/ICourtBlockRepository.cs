using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ICourtBlockRepository
{
    Task<IReadOnlyCollection<CourtBlock>> GetForCourtAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
