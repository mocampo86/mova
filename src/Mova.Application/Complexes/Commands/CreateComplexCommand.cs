namespace Mova.Application.Complexes.Commands;

public sealed record CreateComplexCommand(
    Guid UserId,
    string Name,
    string Description,
    string Address,
    string City,
    decimal? Latitude,
    decimal? Longitude,
    string PhoneNumber,
    string Email,
    int UtcOffsetMinutes = 0);
