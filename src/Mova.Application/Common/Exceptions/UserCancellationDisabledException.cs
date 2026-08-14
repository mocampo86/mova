namespace Mova.Application.Common.Exceptions;

public sealed class UserCancellationDisabledException(string message) : Exception(message);
