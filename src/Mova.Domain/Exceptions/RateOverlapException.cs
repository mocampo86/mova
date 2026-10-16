namespace Mova.Domain.Exceptions;

public sealed class RateOverlapException : DomainException
{
    public RateOverlapException()
        : base("The rate overlaps an existing active rate for the same day and time range.")
    {
    }
}
