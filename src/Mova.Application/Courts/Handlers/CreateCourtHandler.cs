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
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourtHandler(ISportsComplexRepository complexes, ICourtRepository courts, IUnitOfWork unitOfWork)
        => (_complexes, _courts, _unitOfWork) = (complexes, courts, unitOfWork);

    public async Task<CourtInfo> HandleAsync(CreateCourtCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _complexes.GetByIdAsync(command.SportsComplexId, cancellationToken) ?? throw new NotFoundException("Sports complex not found.");
        if (await _courts.ExistsByNameAsync(command.SportsComplexId, command.Name, cancellationToken))
            throw new ConflictException("A court with this name already exists in the sports complex.");

        var court = Court.Create(command.SportsComplexId, command.Name, command.Description, command.SurfaceType, command.Indoor, command.SportIds);
        await _courts.AddAsync(court, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CourtMapper.ToInfo(court);
    }
}
