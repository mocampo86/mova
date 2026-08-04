namespace Mova.Domain.Exceptions;

public sealed class UserBlockedException : DomainException
{
    public UserBlockedException()
        : base("User is blocked.")
    {
    }
}
