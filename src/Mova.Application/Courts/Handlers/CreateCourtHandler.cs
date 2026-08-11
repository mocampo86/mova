using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts.Handlers;

public sealed class CreateCourtHandler : ICreateCourtHandler
{
    private readonly ISportsComplexRepository _complexes;
    private readonly ICourtRepository _courts;
    private readonly ISportRepository _sports;
    private readonly ICourtAvailabilityRuleRepository _rules;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourtHandler(ISportsComplexRepository complexes, ICourtRepository courts, ISportRepository sports, ICourtAvailabilityRuleRepository rules, IUnitOfWork unitOfWork)
        => (_complexes, _courts, _sports, _rules, _unitOfWork) = (complexes, courts, sports, rules, unitOfWork);

    public async Task<CourtInfo> HandleAsync(CreateCourtCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _complexes.GetByIdAsync(command.SportsComplexId, cancellationToken) ?? throw new NotFoundException("Sports complex not found.");
        if (await _courts.ExistsByNameAsync(command.SportsComplexId, command.Name, cancellationToken))
            throw new ConflictException("A court with this name already exists in the sports complex.");

        var sportIds = (command.SportIds ?? []).Distinct().ToArray();
        var existingSportIds = (await _sports.GetByIdsAsync(sportIds, cancellationToken)).Select(x => x.Id).ToHashSet();
        if (sportIds.Any(x => !existingSportIds.Contains(x)))
            throw new NotFoundException("One or more sports were not found.");

        var court = Court.Create(command.SportsComplexId, command.Name, command.Description, command.SurfaceType, command.Indoor, sportIds);
        await _courts.AddAsync(court, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var rule = CourtAvailabilityRule.Create(court.Id, day, TimeSpan.FromHours(8), TimeSpan.FromHours(22), 60, true);
            await _rules.AddAsync(rule, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CourtMapper.ToInfo(court);
    }
}
