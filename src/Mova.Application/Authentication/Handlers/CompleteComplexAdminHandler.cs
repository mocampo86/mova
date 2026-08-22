using System.Data;
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
    private readonly IBusinessHoursRepository _businessHours;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteComplexAdminHandler(
        IUserRepository userRepository,
        ISportsComplexRepository sportsComplexRepository,
        IComplexAdministratorRepository complexAdministratorRepository,
        IBusinessHoursRepository businessHours,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _sportsComplexRepository = sportsComplexRepository;
        _complexAdministratorRepository = complexAdministratorRepository;
        _businessHours = businessHours;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public Task<CompleteComplexAdminResponse> HandleAsync(
        CompleteComplexAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                var user = await _userRepository.GetByIdAsync(command.UserId, token)
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

                await _sportsComplexRepository.AddAsync(sportsComplex, token);
                await _complexAdministratorRepository.AddAsync(administrator, token);
                await CreateDefaultBusinessHoursAsync(sportsComplex.Id, token);

                await _unitOfWork.SaveChangesAsync(token);

                var roles = user.Roles.Select(r => r.ToString()).ToList();
                var associations = (await _complexAdministratorRepository.GetByUserIdAsync(user.Id, token))
                    .Select(a => new UserComplexAssociation(a.SportsComplexId, a.Role.ToString()))
                    .ToList();

                var authToken = await _jwtTokenService.GenerateAsync(user, roles, associations, token);

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
            },
            IsolationLevel.ReadCommitted,
            cancellationToken);
    }

    private async Task CreateDefaultBusinessHoursAsync(Guid sportsComplexId, CancellationToken token)
    {
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var hours = BusinessHours.Create(sportsComplexId, day, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);
            await _businessHours.AddAsync(hours, token);
        }
    }
}
