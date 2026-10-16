using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Handlers;

public interface IAddFavoriteComplexHandler
{
    Task HandleAsync(AddFavoriteComplexCommand command, CancellationToken cancellationToken = default);
}
