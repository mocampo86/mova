using Mova.Application.Courts.Queries;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IGetCourtAvailabilityHandler
{
    Task<IReadOnlyCollection<CourtAvailabilitySlotInfo>> HandleAsync(GetCourtAvailabilityQuery query, CancellationToken cancellationToken = default);
}
