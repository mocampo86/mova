using Mova.Application.Users.Queries;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface IGetMyBlockStatusHandler
{
    Task<MyBlockStatusInfo> HandleAsync(GetMyBlockStatusQuery query, CancellationToken cancellationToken = default);
}
