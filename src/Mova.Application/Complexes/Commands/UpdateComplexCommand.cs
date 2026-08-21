namespace Mova.Application.Complexes.Commands;

public sealed record UpdateComplexCommand(
    Guid ComplexId,
    string Name,
    string Description,
    string Address,
    string City,
    decimal? Latitude,
    decimal? Longitude,
    string PhoneNumber,
    string Email);
