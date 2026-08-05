using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;
using Mova.Domain.Enums;

namespace Mova.Application.Complexes.Handlers;

public sealed class UpdateComplexStatusHandler : IUpdateComplexStatusHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComplexStatusHandler(ISportsComplexRepository sportsComplexRepository, IUnitOfWork unitOfWork)
    {
        _sportsComplexRepository = sportsComplexRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SportsComplexInfo> HandleAsync(UpdateComplexStatusCommand command, CancellationToken cancellationToken = default)
    {
        var sportsComplex = await _sportsComplexRepository.GetByIdAsync(command.ComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        if (command.Status == ComplexStatus.Active)
        {
            sportsComplex.Activate();
        }
        else
        {
            sportsComplex.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SportsComplexInfoMapper.ToInfo(sportsComplex);
    }
}
