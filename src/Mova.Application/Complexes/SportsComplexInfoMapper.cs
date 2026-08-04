using Mova.Domain.Entities;
using Mova.Contracts.Complexes;

namespace Mova.Application.Complexes;

public static class SportsComplexInfoMapper
{
    public static SportsComplexInfo ToInfo(SportsComplex sportsComplex)
    {
        return new SportsComplexInfo
        {
            Id = sportsComplex.Id,
            Name = sportsComplex.Name,
            Description = sportsComplex.Description,
            Address = sportsComplex.Address,
            City = sportsComplex.City,
            Latitude = sportsComplex.Latitude,
            Longitude = sportsComplex.Longitude,
            PhoneNumber = sportsComplex.PhoneNumber,
            Email = sportsComplex.Email,
            Status = sportsComplex.Status.ToString(),
            CreatedAt = sportsComplex.CreatedAt,
            UpdatedAt = sportsComplex.UpdatedAt
        };
    }
}
