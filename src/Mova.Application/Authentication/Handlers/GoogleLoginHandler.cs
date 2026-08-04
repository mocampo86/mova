using Mova.Application.Abstractions.Authentication;
using Mova.Application.Abstractions.Persistence;
using Mova.Application.Authentication.Commands;
using Mova.Application.Authentication.Models;
using Mova.Application.Common.Exceptions;
using Mova.Contracts.Auth;
using Mova.Contracts.Users;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Exceptions;

namespace Mova.Application.Authentication.Handlers;

public sealed class GoogleLoginHandler : IGoogleLoginHandler
{
    private readonly IGoogleTokenValidator _tokenValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IComplexAdministratorRepository _complexAdministratorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GoogleLoginHandler(
        IGoogleTokenValidator tokenValidator,
        IJwtTokenService jwtTokenService,
        IUserRepository userRepository,
        IComplexAdministratorRepository complexAdministratorRepository,
        IUnitOfWork unitOfWork)
    {
        _tokenValidator = tokenValidator;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _complexAdministratorRepository = complexAdministratorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoogleLoginResponse> HandleAsync(GoogleLoginCommand command, CancellationToken cancellationToken = default)
    {
        var token = await _tokenValidator.ValidateAsync(command.IdToken, cancellationToken);

        var user = await _userRepository.GetByGoogleSubjectIdAsync(token.Subject, cancellationToken)
            ?? await _userRepository.GetByEmailAsync(token.Email, cancellationToken);

        if (user is null)
        {
            user = User.CreateFromGoogle(Guid.NewGuid(), token.Subject, token.Email, token.Name);
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            user.UpdateProfile(token.Name);
        }

        if (user.Status == UserStatus.Blocked)
        {
            throw new UserBlockedException();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.Roles.Select(r => r.ToString()).ToList();
        var complexAssociations = await _complexAdministratorRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var complexClaims = complexAssociations
            .Select(a => new UserComplexAssociation(a.SportsComplexId, a.Role.ToString()))
            .ToList();

        var authToken = await _jwtTokenService.GenerateAsync(user, roles, complexClaims, cancellationToken);

        return new GoogleLoginResponse
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
            RequiresProfileCompletion = string.IsNullOrWhiteSpace(user.PhoneNumber)
        };
    }
}
