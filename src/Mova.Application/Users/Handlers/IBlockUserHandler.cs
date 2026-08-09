using Mova.Application.Users.Commands;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface IBlockUserHandler
{
    Task<BlockedUserInfo> HandleAsync(BlockUserCommand command, CancellationToken cancellationToken = default);
}
