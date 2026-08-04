using Mova.Application.Authentication.Models;
using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    Task<AuthToken> GenerateAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
