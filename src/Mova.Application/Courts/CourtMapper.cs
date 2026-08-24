using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts;

public static class CourtMapper
{
    public static CourtInfo ToInfo(Court court) => new()
    {
        Id = court.Id, SportsComplexId = court.SportsComplexId, Name = court.Name,
        Description = court.Description, SurfaceType = court.SurfaceType, Indoor = court.Indoor,
        Status = court.Status.ToString(), CreatedAt = court.CreatedAt, UpdatedAt = court.UpdatedAt,
        SportIds = court.CourtSports.Select(x => x.SportId).ToArray()
    };

    public static CourtInfo ToPublicInfo(Court court)
    {
        var info = ToInfo(court);
        info.Status = null;
        return info;
    }
}
