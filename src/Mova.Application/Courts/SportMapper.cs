using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts;

public static class SportMapper
{
    public static SportInfo ToInfo(Sport sport) => new()
    {
        Id = sport.Id,
        Name = sport.Name
    };
}
