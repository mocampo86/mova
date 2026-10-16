using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Users.Handlers;

public sealed class AddFavoriteComplexHandler(
    IUserFavoriteComplexRepository favorites,
    ISportsComplexRepository sportsComplexes) : IAddFavoriteComplexHandler
{
    public async Task HandleAsync(AddFavoriteComplexCommand command, CancellationToken cancellationToken = default)
    {
        var complex = await sportsComplexes.GetByIdAsync(command.SportsComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        if (complex.Status != ComplexStatus.Active)
        {
            throw new ConflictException("This sports complex is not available.");
        }

        var favorite = UserFavoriteComplex.Create(command.UserId, command.SportsComplexId);
        await favorites.AddIfNotExistsAsync(favorite, cancellationToken);
    }
}
