using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IUpdateRecurringReservationSettingsHandler
{
    Task<SportsComplexInfo> HandleAsync(UpdateRecurringReservationSettingsCommand command, CancellationToken cancellationToken = default);
}
