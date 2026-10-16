using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public sealed record FavoriteComplexItem(
    UserFavoriteComplex Favorite,
    SportsComplex SportsComplex,
    IReadOnlyList<string> SportNames);

public interface IUserFavoriteComplexRepository
{
    Task<IReadOnlyCollection<Guid>> GetComplexIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<FavoriteComplexItem> Items, int TotalItems)> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default);
    Task<bool> AddIfNotExistsAsync(UserFavoriteComplex favorite, CancellationToken cancellationToken = default);
    Task<bool> RemoveIfExistsAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default);
}
