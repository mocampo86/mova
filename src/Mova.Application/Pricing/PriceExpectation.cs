using Mova.Application.Common.Exceptions;

namespace Mova.Application.Pricing;

public static class PriceExpectation
{
    public static void EnsureMatches(decimal? expectedTotalPrice, string? expectedCurrency, decimal actualTotalPrice, string actualCurrency)
    {
        if (expectedTotalPrice is null && expectedCurrency is null)
        {
            return;
        }

        if (expectedTotalPrice != actualTotalPrice ||
            !string.Equals(expectedCurrency, actualCurrency, StringComparison.OrdinalIgnoreCase))
        {
            throw new PriceChangedException(expectedTotalPrice, expectedCurrency, actualTotalPrice, actualCurrency);
        }
    }
}
