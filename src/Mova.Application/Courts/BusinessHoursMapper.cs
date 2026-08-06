using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts;

public static class BusinessHoursMapper
{
    public static BusinessHoursInfo ToInfo(BusinessHours hours) => new()
    {
        Id = hours.Id,
        SportsComplexId = hours.SportsComplexId,
        DayOfWeek = hours.DayOfWeek,
        OpeningTime = hours.OpeningTime,
        ClosingTime = hours.ClosingTime,
        IsClosed = hours.IsClosed
    };
}
