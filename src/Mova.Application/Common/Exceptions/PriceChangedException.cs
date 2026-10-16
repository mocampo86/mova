namespace Mova.Application.Common.Exceptions;

public sealed class PriceChangedException : Exception
{
    public PriceChangedException(decimal? expectedTotalPrice, string? expectedCurrency, decimal actualTotalPrice, string actualCurrency)
        : base("The price changed since it was quoted. Please review the updated price.")
    {
        ExpectedTotalPrice = expectedTotalPrice;
        ExpectedCurrency = expectedCurrency;
        ActualTotalPrice = actualTotalPrice;
        ActualCurrency = actualCurrency;
    }

    public decimal? ExpectedTotalPrice { get; }
    public string? ExpectedCurrency { get; }
    public decimal ActualTotalPrice { get; }
    public string ActualCurrency { get; }
}
