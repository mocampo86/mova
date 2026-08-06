using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public interface IUpdateBusinessHoursHandler
{
    Task<IReadOnlyCollection<BusinessHoursInfo>> HandleAsync(UpdateBusinessHoursCommand command, CancellationToken cancellationToken = default);
}
