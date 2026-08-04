namespace Mova.Application.Users.Commands;

public sealed record CompleteProfileCommand(Guid UserId, string PhoneNumber);
