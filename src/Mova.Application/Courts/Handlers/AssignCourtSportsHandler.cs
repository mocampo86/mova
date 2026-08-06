using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Courts.Commands;
using Mova.Contracts.Courts;

namespace Mova.Application.Courts.Handlers;

public sealed class AssignCourtSportsHandler(
    ICourtRepository courts,
    ISportRepository sports,
    IUnitOfWork unitOfWork) : IAssignCourtSportsHandler
{
    public async Task<CourtInfo> HandleAsync(AssignCourtSportsCommand command, CancellationToken cancellationToken = default)
    {
        var court = await courts.GetByIdAsync(command.CourtId, cancellationToken)
            ?? throw new NotFoundException("Court not found.");
        if (court.SportsComplexId != command.SportsComplexId)
            throw new NotFoundException("Court not found.");

        var requestedIds = command.SportIds.Distinct().ToArray();
        var existingSports = await sports.GetByIdsAsync(requestedIds, cancellationToken);
        var missingIds = requestedIds.Except(existingSports.Select(x => x.Id)).ToArray();
        if (missingIds.Length > 0)
            throw new NotFoundException("One or more sports were not found.");

        court.AssignSports(requestedIds);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CourtMapper.ToInfo(court);
    }
}
