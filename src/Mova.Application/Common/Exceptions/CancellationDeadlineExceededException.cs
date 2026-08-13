namespace Mova.Application.Common.Exceptions;

public sealed class CancellationDeadlineExceededException(string message) : Exception(message);
