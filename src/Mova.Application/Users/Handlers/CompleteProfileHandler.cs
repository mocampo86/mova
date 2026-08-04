using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Users.Commands;
using Mova.Contracts.Users;

namespace Mova.Application.Users.Handlers;

public sealed class CompleteProfileHandler : ICompleteProfileHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteProfileHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserInfo> HandleAsync(CompleteProfileCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.CompleteProfile(command.PhoneNumber);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            PhoneVerified = user.PhoneVerified
        };
    }
}
