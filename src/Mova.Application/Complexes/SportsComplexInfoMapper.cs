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
            AllowUserRecurringReservations = sportsComplex.AllowUserRecurringReservations,
            UtcOffsetMinutes = sportsComplex.UtcOffsetMinutes,
            CreatedAt = sportsComplex.CreatedAt,
            UpdatedAt = sportsComplex.UpdatedAt
        };
    }

    public static SportsComplexInfo ToPublicInfo(SportsComplex sportsComplex)
    {
        var info = ToInfo(sportsComplex);
        info.Status = null;
        return info;
    }
}
