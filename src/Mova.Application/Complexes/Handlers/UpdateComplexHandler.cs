using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;
using Mova.Domain.Entities;

namespace Mova.Application.Complexes.Handlers;

public sealed class UpdateComplexHandler : IUpdateComplexHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComplexHandler(ISportsComplexRepository sportsComplexRepository, IUnitOfWork unitOfWork)
    {
        _sportsComplexRepository = sportsComplexRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SportsComplexInfo> HandleAsync(UpdateComplexCommand command, CancellationToken cancellationToken = default)
    {
        var sportsComplex = await _sportsComplexRepository.GetByIdAsync(command.ComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        sportsComplex.Update(
            command.Name,
            command.Description,
            command.Address,
            command.City,
            command.Latitude,
            command.Longitude,
            command.PhoneNumber,
            command.Email,
            command.UtcOffsetMinutes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SportsComplexInfoMapper.ToInfo(sportsComplex);
    }
}
