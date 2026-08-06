using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface ISportRepository
{
    Task<IReadOnlyCollection<Sport>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
