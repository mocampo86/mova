using Mova.Application.Users.Commands;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public interface ICompleteProfileHandler
{
    Task<UserInfo> HandleAsync(CompleteProfileCommand command, CancellationToken cancellationToken = default);
}
