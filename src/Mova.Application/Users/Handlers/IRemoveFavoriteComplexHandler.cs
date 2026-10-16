using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Handlers;

public interface IRemoveFavoriteComplexHandler
{
    Task HandleAsync(RemoveFavoriteComplexCommand command, CancellationToken cancellationToken = default);
}
