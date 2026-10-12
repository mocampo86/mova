namespace Mova.Domain.Exceptions;

public sealed class PriceNotConfiguredException : DomainException
{
    public PriceNotConfiguredException(IReadOnlyList<PriceGap> gaps)
        : base("No rate is configured for part of the requested time range.")
    {
        Gaps = gaps;
    }

    public PriceNotConfiguredException()
        : this([])
    {
    }

    public IReadOnlyList<PriceGap> Gaps { get; }
}

public sealed record PriceGap(DateTime StartAt, DateTime EndAt);
