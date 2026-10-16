using Mova.Application.Abstractions.Persistence;
using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public sealed class GetMyFavoriteComplexesHandler(IUserFavoriteComplexRepository favorites) : IGetMyFavoriteComplexesHandler
{
    public async Task<PagedResult<FavoriteComplexInfo>> HandleAsync(GetMyFavoriteComplexesQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalItems) = await favorites.GetByUserIdAsync(query.UserId, query.Page, query.PageSize, cancellationToken);

        var mapped = items
            .Select(i => new FavoriteComplexInfo
            {
                Id = i.SportsComplex.Id,
                Name = i.SportsComplex.Name,
                Description = i.SportsComplex.Description,
                Address = i.SportsComplex.Address,
                City = i.SportsComplex.City,
                Status = i.SportsComplex.Status.ToString(),
                Sports = i.SportNames,
                CreatedAt = i.Favorite.CreatedAt
            })
            .ToList();

        return PagedResult<FavoriteComplexInfo>.Create(mapped, query.Page, query.PageSize, totalItems);
    }
}
