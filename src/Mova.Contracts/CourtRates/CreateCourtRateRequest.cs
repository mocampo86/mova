namespace Mova.Contracts.CourtRates;

public sealed class CreateCourtRateRequest
{
    public IReadOnlyCollection<int> DaysOfWeek { get; set; } = [];
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal AmountPerHour { get; set; }
}
