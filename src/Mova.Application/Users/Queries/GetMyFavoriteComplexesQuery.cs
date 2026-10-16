namespace Mova.Application.Users.Queries;

public sealed record GetMyFavoriteComplexesQuery(Guid UserId, int Page, int PageSize);
