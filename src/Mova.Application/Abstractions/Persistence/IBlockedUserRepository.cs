namespace Mova.Application.Abstractions.Persistence;

public interface IBlockedUserRepository
{
    Task<int> CountActiveByComplexIdAsync(Guid sportsComplexId, CancellationToken cancellationToken = default);
}
