using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Complexes.Handlers;

public sealed class CreateComplexHandler : ICreateComplexHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ISportsComplexRepository _sportsComplexRepository;
    private readonly IComplexAdministratorRepository _complexAdministratorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateComplexHandler(
        IUserRepository userRepository,
        ISportsComplexRepository sportsComplexRepository,
        IComplexAdministratorRepository complexAdministratorRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _sportsComplexRepository = sportsComplexRepository;
        _complexAdministratorRepository = complexAdministratorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SportsComplexInfo> HandleAsync(CreateComplexCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var sportsComplex = SportsComplex.Create(
            command.Name,
            command.Description,
            command.Address,
            command.City,
            command.Latitude,
            command.Longitude,
            command.PhoneNumber,
            command.Email);

        var administrator = ComplexAdministrator.Create(sportsComplex.Id, user.Id, Role.ComplexAdmin);

        user.AddRole(Role.ComplexAdmin);

        await _sportsComplexRepository.AddAsync(sportsComplex, cancellationToken);
        await _complexAdministratorRepository.AddAsync(administrator, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SportsComplexInfoMapper.ToInfo(sportsComplex);
    }
}
