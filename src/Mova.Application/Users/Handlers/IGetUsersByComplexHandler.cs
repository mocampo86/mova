using Mova.Application.Users.Queries;
using Mova.Contracts.Common;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface IGetUsersByComplexHandler
{
    Task<PagedResult<ComplexUserInfo>> HandleAsync(GetUsersByComplexQuery query, CancellationToken cancellationToken = default);
}
