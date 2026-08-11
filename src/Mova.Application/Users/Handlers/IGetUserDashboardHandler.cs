using Mova.Application.Users.Queries;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface IGetUserDashboardHandler
{
    Task<UserDashboardInfo> HandleAsync(GetUserDashboardQuery query, CancellationToken cancellationToken = default);
}
