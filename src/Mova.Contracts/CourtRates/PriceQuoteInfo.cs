namespace Mova.Contracts.CourtRates;

public sealed class PriceQuoteInfo
{
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsFullyCovered { get; set; }
    public IReadOnlyCollection<PriceQuoteLineInfo> Lines { get; set; } = [];
    public IReadOnlyCollection<PriceGapInfo> Gaps { get; set; } = [];
}

public sealed class PriceQuoteLineInfo
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal Amount { get; set; }
}

public sealed class PriceGapInfo
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
