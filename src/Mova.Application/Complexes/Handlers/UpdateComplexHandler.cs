using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Complexes.Commands;
using Mova.Contracts.Complexes;
using Mova.Domain.Entities;

namespace Mova.Application.Complexes.Handlers;

public sealed class UpdateComplexHandler : IUpdateComplexHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;
    private readonly IAuditLogRepository _auditLogs;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComplexHandler(
        ISportsComplexRepository sportsComplexRepository,
        IAuditLogRepository auditLogs,
        ICurrentUserContext currentUser,
        IUnitOfWork unitOfWork)
    {
        _sportsComplexRepository = sportsComplexRepository;
        _auditLogs = auditLogs;
        _currentUser = currentUser;
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
            command.TimeZoneId);

        var auditLog = AuditLog.Create(
            _currentUser.UserId,
            sportsComplex.Id,
            "SportsComplex.Update",
            "SportsComplex",
            sportsComplex.Id.ToString(),
            new { name = sportsComplex.Name, city = sportsComplex.City, timeZoneId = sportsComplex.TimeZoneId });

        await _auditLogs.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SportsComplexInfoMapper.ToInfo(sportsComplex);
    }
}
