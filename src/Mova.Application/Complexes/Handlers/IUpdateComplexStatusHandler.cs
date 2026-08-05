using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IUpdateComplexStatusHandler
{
    Task<SportsComplexInfo> HandleAsync(UpdateComplexStatusCommand command, CancellationToken cancellationToken = default);
}
