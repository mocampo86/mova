using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Authentication.Commands;
using Mova.Application.Authentication.Models;
using Mova.Application.Common.Exceptions;
using Mova.Contracts.Auth;
using Mova.Contracts.Users;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Application.Authentication.Handlers;

public sealed class CompleteComplexAdminHandler : ICompleteComplexAdminHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ISportsComplexRepository _sportsComplexRepository;
    private readonly IComplexAdministratorRepository _complexAdministratorRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteComplexAdminHandler(
        IUserRepository userRepository,
        ISportsComplexRepository sportsComplexRepository,
        IComplexAdministratorRepository complexAdministratorRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _sportsComplexRepository = sportsComplexRepository;
        _complexAdministratorRepository = complexAdministratorRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompleteComplexAdminResponse> HandleAsync(
        CompleteComplexAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.CompleteProfile(command.PhoneNumber);
        user.AddRole(Role.ComplexAdmin);

        var sportsComplex = SportsComplex.Create(
            command.Name,
            command.Description,
            command.Address,
            command.City,
            command.Latitude,
            command.Longitude,
            command.ComplexPhoneNumber,
            command.ComplexEmail,
            ComplexStatus.Pending);

        var administrator = ComplexAdministrator.Create(sportsComplex.Id, user.Id, Role.ComplexAdmin);

        await _sportsComplexRepository.AddAsync(sportsComplex, cancellationToken);
        await _complexAdministratorRepository.AddAsync(administrator, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.Roles.Select(r => r.ToString()).ToList();
        var associations = (await _complexAdministratorRepository.GetByUserIdAsync(user.Id, cancellationToken))
            .Select(a => new UserComplexAssociation(a.SportsComplexId, a.Role.ToString()))
            .ToList();

        var authToken = await _jwtTokenService.GenerateAsync(user, roles, associations, cancellationToken);

        return new CompleteComplexAdminResponse
        {
            AccessToken = authToken.AccessToken,
            ExpiresAt = authToken.ExpiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                PhoneVerified = user.PhoneVerified
            },
            ComplexId = sportsComplex.Id,
            RequiresProfileCompletion = false
        };
    }
}
