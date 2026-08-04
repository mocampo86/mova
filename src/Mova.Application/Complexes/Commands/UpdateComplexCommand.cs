using Mova.Domain.Enums;

namespace Mova.Application.Complexes.Commands;

public sealed record UpdateComplexCommand(
    Guid ComplexId,
    Guid UserId,
    string Name,
    string Description,
    string Address,
    string City,
    decimal? Latitude,
    decimal? Longitude,
    string PhoneNumber,
    string Email,
    ComplexStatus Status);
