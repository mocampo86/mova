namespace Mova.Domain.Exceptions;

public sealed class UnresolvedTimeZoneException : DomainException
{
    public UnresolvedTimeZoneException()
        : base("The sports complex timezone is not configured. Please configure a valid IANA timezone before using this feature.")
    {
    }
}
