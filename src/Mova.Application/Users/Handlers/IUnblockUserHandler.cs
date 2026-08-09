using Mova.Application.Users.Commands;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface IUnblockUserHandler
{
    Task<BlockedUserInfo> HandleAsync(UnblockUserCommand command, CancellationToken cancellationToken = default);
}
