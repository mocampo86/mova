using System.Data;
using Mova.Application.Abstractions.Authentication;
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
    private readonly IBusinessHoursRepository _businessHours;
    private readonly IAuditLogRepository _auditLogs;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateComplexHandler(
        IUserRepository userRepository,
        ISportsComplexRepository sportsComplexRepository,
        IComplexAdministratorRepository complexAdministratorRepository,
        IBusinessHoursRepository businessHours,
        IAuditLogRepository auditLogs,
        ICurrentUserContext currentUser,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _sportsComplexRepository = sportsComplexRepository;
        _complexAdministratorRepository = complexAdministratorRepository;
        _businessHours = businessHours;
        _auditLogs = auditLogs;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<SportsComplexInfo> HandleAsync(CreateComplexCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return await _unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                var sportsComplex = SportsComplex.Create(
                    command.Name,
                    command.Description,
                    command.Address,
                    command.City,
                    command.Latitude,
                    command.Longitude,
                    command.PhoneNumber,
                    command.Email,
                    command.TimeZoneId);

                var administrator = ComplexAdministrator.Create(sportsComplex.Id, user.Id, Role.ComplexAdmin);

                user.AddRole(Role.ComplexAdmin);

                await _sportsComplexRepository.AddAsync(sportsComplex, token);
                await _complexAdministratorRepository.AddAsync(administrator, token);
                await CreateDefaultBusinessHoursAsync(sportsComplex.Id, token);

                var auditLog = AuditLog.Create(
                    _currentUser.UserId,
                    sportsComplex.Id,
                    "SportsComplex.Create",
                    "SportsComplex",
                    sportsComplex.Id.ToString(),
                    new { name = sportsComplex.Name, city = sportsComplex.City, timeZoneId = sportsComplex.TimeZoneId });

                await _auditLogs.AddAsync(auditLog, token);
                await _unitOfWork.SaveChangesAsync(token);

                return SportsComplexInfoMapper.ToInfo(sportsComplex);
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
