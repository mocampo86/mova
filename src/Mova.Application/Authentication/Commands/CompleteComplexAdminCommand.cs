namespace Mova.Application.Authentication.Commands;

public sealed record CompleteComplexAdminCommand(
    Guid UserId,
    string PhoneNumber,
    string Name,
    string Description,
    string Address,
    string City,
    decimal? Latitude,
    decimal? Longitude,
    string ComplexPhoneNumber,
    string ComplexEmail);
