using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface IGetMyFavoriteComplexesHandler
{
    Task<PagedResult<FavoriteComplexInfo>> HandleAsync(GetMyFavoriteComplexesQuery query, CancellationToken cancellationToken = default);
}
