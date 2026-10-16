using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Application.Users.Handlers;
using Mova.Application.Users.Queries;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public class UserFavoriteComplexHandlerTests
{
    private static SportsComplex CreateComplex()
    {
        return SportsComplex.Create(
            $"Complex {Guid.NewGuid()}",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            $"complex-{Guid.NewGuid()}@test.com");
    }

    [Fact]
    public async Task AddFavorite_WithActiveComplex_AddsRelation()
    {
        var favorites = new FakeUserFavoriteComplexRepository();
        var complexes = new FakeSportsComplexRepository();
        var complex = CreateComplex();
        await complexes.AddAsync(complex);
        var handler = new AddFavoriteComplexHandler(favorites, complexes);
        var userId = Guid.NewGuid();

        await handler.HandleAsync(new AddFavoriteComplexCommand(userId, complex.Id));

        Assert.Single(favorites.Favorites);
        Assert.Equal(userId, favorites.Favorites[0].UserId);
        Assert.Equal(complex.Id, favorites.Favorites[0].SportsComplexId);
    }

    [Fact]
    public async Task AddFavorite_WithNonexistentComplex_ThrowsNotFound()
    {
        var favorites = new FakeUserFavoriteComplexRepository();
        var complexes = new FakeSportsComplexRepository();
        var handler = new AddFavoriteComplexHandler(favorites, complexes);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(new AddFavoriteComplexCommand(Guid.NewGuid(), Guid.NewGuid())));

        Assert.Empty(favorites.Favorites);
    }

    [Fact]
    public async Task AddFavorite_WithInactiveComplex_ThrowsConflict()
    {
        var favorites = new FakeUserFavoriteComplexRepository();
        var complexes = new FakeSportsComplexRepository();
        var complex = CreateComplex();
        complex.Deactivate();
        await complexes.AddAsync(complex);
        var handler = new AddFavoriteComplexHandler(favorites, complexes);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(new AddFavoriteComplexCommand(Guid.NewGuid(), complex.Id)));

        Assert.Empty(favorites.Favorites);
    }

    [Fact]
    public async Task AddFavorite_WhenAlreadyFavorite_IsIdempotent()
    {
        var favorites = new FakeUserFavoriteComplexRepository();
        var complexes = new FakeSportsComplexRepository();
        var complex = CreateComplex();
        await complexes.AddAsync(complex);
        var handler = new AddFavoriteComplexHandler(favorites, complexes);
        var command = new AddFavoriteComplexCommand(Guid.NewGuid(), complex.Id);

        await handler.HandleAsync(command);
        await handler.HandleAsync(command);

        Assert.Single(favorites.Favorites);
    }

    [Fact]
    public async Task RemoveFavorite_RemovesOnlyOwnRelation()
    {
        var favorites = new FakeUserFavoriteComplexRepository();
        var complexId = Guid.NewGuid();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await favorites.AddIfNotExistsAsync(UserFavoriteComplex.Create(userA, complexId));
        await favorites.AddIfNotExistsAsync(UserFavoriteComplex.Create(userB, complexId));
        var handler = new RemoveFavoriteComplexHandler(favorites);

        await handler.HandleAsync(new RemoveFavoriteComplexCommand(userA, complexId));

        Assert.Single(favorites.Favorites);
        Assert.Equal(userB, favorites.Favorites[0].UserId);
    }

    [Fact]
    public async Task RemoveFavorite_WhenNotFavorite_IsIdempotent()
    {
        var favorites = new FakeUserFavoriteComplexRepository();
        var handler = new RemoveFavoriteComplexHandler(favorites);

        await handler.HandleAsync(new RemoveFavoriteComplexCommand(Guid.NewGuid(), Guid.NewGuid()));
        await handler.HandleAsync(new RemoveFavoriteComplexCommand(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Empty(favorites.Favorites);
    }

    [Fact]
    public async Task GetFavoriteIds_ReturnsOnlyCurrentUserIds()
    {
        var favorites = new FakeUserFavoriteComplexRepository();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var complex1 = Guid.NewGuid();
        var complex2 = Guid.NewGuid();
        await favorites.AddIfNotExistsAsync(UserFavoriteComplex.Create(userA, complex1));
        await favorites.AddIfNotExistsAsync(UserFavoriteComplex.Create(userA, complex2));
        await favorites.AddIfNotExistsAsync(UserFavoriteComplex.Create(userB, complex1));
        var handler = new GetMyFavoriteComplexIdsHandler(favorites);

        var result = await handler.HandleAsync(new GetMyFavoriteComplexIdsQuery(userA));

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(complex1, result.Items);
        Assert.Contains(complex2, result.Items);
    }

    [Fact]
    public async Task FavoriteEntity_WithEmptyIds_Throws()
    {
        Assert.Throws<ArgumentException>(() => UserFavoriteComplex.Create(Guid.Empty, Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => UserFavoriteComplex.Create(Guid.NewGuid(), Guid.Empty));
    }
}
