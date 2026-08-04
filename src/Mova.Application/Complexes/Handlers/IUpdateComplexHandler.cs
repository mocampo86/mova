using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface IUpdateComplexHandler
{
    Task<SportsComplexInfo> HandleAsync(UpdateComplexCommand command, CancellationToken cancellationToken = default);
}
