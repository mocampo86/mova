using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;
using Mova.Domain.Entities;

namespace Mova.Application.Courts.Handlers;

public sealed class UpdateCourtHandler(
    ISportsComplexRepository complexes,
    ICourtRepository courts,
    ISportRepository sports,
    IUnitOfWork unitOfWork) : IUpdateCourtHandler
{
    public async Task<CourtInfo> HandleAsync(UpdateCourtCommand command, CancellationToken cancellationToken = default)
    {
        _ = await complexes.GetByIdAsync(command.SportsComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        var court = await courts.GetByIdAsync(command.CourtId, cancellationToken)
            ?? throw new NotFoundException("Court not found.");

        if (court.SportsComplexId != command.SportsComplexId)
            throw new NotFoundException("Court not found.");

        if (await courts.ExistsByNameAsync(command.SportsComplexId, command.Name, command.CourtId, cancellationToken))
            throw new ConflictException("A court with this name already exists in the sports complex.");

        var sportIds = command.SportIds?.Distinct().ToArray();
        if (sportIds is not null && sportIds.Length > 0)
        {
            var existingSportIds = (await sports.GetByIdsAsync(sportIds, cancellationToken)).Select(x => x.Id).ToHashSet();
            if (sportIds.Any(x => !existingSportIds.Contains(x)))
                throw new NotFoundException("One or more sports were not found.");
        }

        court.Update(command.Name, command.Description, command.SurfaceType, command.Indoor, sportIds);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CourtMapper.ToInfo(court);
    }
}
