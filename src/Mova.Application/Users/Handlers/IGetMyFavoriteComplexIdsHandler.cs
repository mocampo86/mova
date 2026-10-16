using Mova.Application.Users.Queries;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface IGetMyFavoriteComplexIdsHandler
{
    Task<FavoriteComplexIdsInfo> HandleAsync(GetMyFavoriteComplexIdsQuery query, CancellationToken cancellationToken = default);
}
