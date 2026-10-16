using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;

namespace Mova.UnitTests.Application.Users;

public sealed class FakeUserFavoriteComplexRepository : IUserFavoriteComplexRepository
{
    private readonly List<UserFavoriteComplex> _favorites = [];

    public IReadOnlyList<UserFavoriteComplex> Favorites => _favorites;

    public Task<IReadOnlyCollection<Guid>> GetComplexIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ids = _favorites
            .Where(x => x.UserId == userId)
            .Select(x => x.SportsComplexId)
            .ToList();
        return Task.FromResult<IReadOnlyCollection<Guid>>(ids);
    }

    public Task<(IReadOnlyList<FavoriteComplexItem> Items, int TotalItems)> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_favorites.Any(x => x.UserId == userId && x.SportsComplexId == sportsComplexId));
    }

    public Task<bool> AddIfNotExistsAsync(UserFavoriteComplex favorite, CancellationToken cancellationToken = default)
    {
        if (_favorites.Any(x => x.UserId == favorite.UserId && x.SportsComplexId == favorite.SportsComplexId))
        {
            return Task.FromResult(false);
        }

        _favorites.Add(favorite);
        return Task.FromResult(true);
    }

    public Task<bool> RemoveIfExistsAsync(Guid userId, Guid sportsComplexId, CancellationToken cancellationToken = default)
    {
        var removed = _favorites.RemoveAll(x => x.UserId == userId && x.SportsComplexId == sportsComplexId);
        return Task.FromResult(removed > 0);
    }
}
