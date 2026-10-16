using Mova.Application.Abstractions.Persistence;
using Mova.Application.Users.Queries;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public sealed class GetMyFavoriteComplexIdsHandler(IUserFavoriteComplexRepository favorites) : IGetMyFavoriteComplexIdsHandler
{
    public async Task<FavoriteComplexIdsInfo> HandleAsync(GetMyFavoriteComplexIdsQuery query, CancellationToken cancellationToken = default)
    {
        var ids = await favorites.GetComplexIdsByUserIdAsync(query.UserId, cancellationToken);
        return new FavoriteComplexIdsInfo { Items = ids.ToList() };
    }
}
