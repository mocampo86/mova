using Mova.Application.Abstractions.Persistence;
using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts.Handlers;

public sealed class UpdateBusinessHoursHandler(
    IBusinessHoursRepository businessHours,
    IUnitOfWork unitOfWork) : IUpdateBusinessHoursHandler
{
    public async Task<IReadOnlyCollection<BusinessHoursInfo>> HandleAsync(UpdateBusinessHoursCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await businessHours.GetBySportsComplexIdAsync(command.SportsComplexId, cancellationToken);
        businessHours.RemoveRange(existing);

        var result = new List<BusinessHours>();
        foreach (var item in command.Hours)
        {
            var hours = BusinessHours.Create(command.SportsComplexId, item.DayOfWeek, item.OpeningTime, item.ClosingTime, item.IsClosed);
            await businessHours.AddAsync(hours, cancellationToken);
            result.Add(hours);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result.Select(BusinessHoursMapper.ToInfo).ToArray();
    }
}
