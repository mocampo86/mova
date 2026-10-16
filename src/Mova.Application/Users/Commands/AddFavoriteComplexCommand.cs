namespace Mova.Application.Users.Commands;

public sealed record AddFavoriteComplexCommand(Guid UserId, Guid SportsComplexId);
