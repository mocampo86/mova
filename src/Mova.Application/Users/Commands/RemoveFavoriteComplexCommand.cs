namespace Mova.Application.Users.Commands;

public sealed record RemoveFavoriteComplexCommand(Guid UserId, Guid SportsComplexId);
