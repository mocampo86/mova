using Mova.Application.Abstractions.Persistence;
using Mova.Application.Users.Commands;

namespace Mova.Application.Users.Handlers;

public sealed class RemoveFavoriteComplexHandler(IUserFavoriteComplexRepository favorites) : IRemoveFavoriteComplexHandler
{
    public async Task HandleAsync(RemoveFavoriteComplexCommand command, CancellationToken cancellationToken = default)
    {
        await favorites.RemoveIfExistsAsync(command.UserId, command.SportsComplexId, cancellationToken);
    }
}
