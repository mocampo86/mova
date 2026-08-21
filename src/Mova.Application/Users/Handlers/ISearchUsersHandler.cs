using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface ISearchUsersHandler
{
    Task<PagedResult<ComplexUserInfo>> HandleAsync(SearchUsersQuery query, CancellationToken cancellationToken = default);
}
