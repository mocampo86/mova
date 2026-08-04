using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes.Handlers;

public interface ICreateComplexHandler
{
    Task<SportsComplexInfo> HandleAsync(CreateComplexCommand command, CancellationToken cancellationToken = default);
}
